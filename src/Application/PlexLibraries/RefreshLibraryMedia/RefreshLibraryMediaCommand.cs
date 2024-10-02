using System.Diagnostics;
using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves the new media metadata from the PlexApi and stores it in the database.
/// </summary>
/// <param name="PlexLibraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
/// <param name="ProgressAction">The action to call for a progress update.</param>
/// <returns>Returns the PlexLibrary with the containing media.</returns>
public record RefreshLibraryMediaCommand(int PlexLibraryId, Action<LibraryProgress>? ProgressAction = null)
    : IRequest<Result<PlexLibrary>>;

public class RefreshLibraryMediaCommandValidator : AbstractValidator<RefreshLibraryMediaCommand> { }

public class RefreshLibraryMediaCommandHandler : IRequestHandler<RefreshLibraryMediaCommand, Result<PlexLibrary>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;
    private readonly IPlexApiService _plexServiceApi;

    private readonly int _baseCountProgress = 1000;
    private readonly int _totalProgressSteps = 5;

    private int _plexLibraryId;
    private Action<LibraryProgress>? _progressAction;

    public RefreshLibraryMediaCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        ISignalRService signalRService,
        IPlexApiService plexServiceApi
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _signalRService = signalRService;
        _plexServiceApi = plexServiceApi;
    }

    public async Task<Result<PlexLibrary>> Handle(
        RefreshLibraryMediaCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexLibrary = await _dbContext
            .PlexLibraries.Include(x => x.PlexServer)
            .FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, cancellationToken);

        if (plexLibrary is null)
            return ResultExtensions.EntityNotFound(nameof(plexLibrary), command.PlexLibraryId);

        _plexLibraryId = plexLibrary.Id;
        _progressAction = command.ProgressAction;

        // Phase 1 of 5:  Retrieve overview of all media belonging to this PlexLibrary
        var newPlexLibraryResult = await _plexServiceApi.GetLibraryMediaAsync(
            plexLibrary,
            progress => SendProgress(1, progress.Percentage, progress.TimeRemaining),
            cancellationToken
        );
        if (newPlexLibraryResult.IsFailed)
            return newPlexLibraryResult;

        var newPlexLibrary = newPlexLibraryResult.Value;

        // Get the default folder path id for the destination
        newPlexLibrary.DefaultDestinationId = newPlexLibrary.Type.ToDefaultDestinationFolderId();

        switch (newPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                return await RefreshPlexMovieLibrary(newPlexLibrary);
            case PlexMediaType.TvShow:
                return await RefreshPlexTvShowLibrary(newPlexLibrary);
            default:
                return Result
                    .Fail($"Library type {newPlexLibrary.Type} is currently not supported by PlexRipper")
                    .LogWarning();
        }
    }

    private async Task<Result<PlexLibrary>> RefreshPlexTvShowLibrary(PlexLibrary plexLibrary)
    {
        if (plexLibrary.Type != PlexMediaType.TvShow)
            return Result.Fail("PlexLibrary is not of type TvShow").LogError();

        if (plexLibrary.TvShows.Any())
        {
            var timer = new Stopwatch();
            timer.Start();

            // Phase 2 of 5: Season data was retrieved successfully.
            var rawSeasonDataResult = await _plexServiceApi.GetAllSeasonsAsync(
                plexLibrary,
                progress => SendProgress(2, progress.Percentage, progress.TimeRemaining)
            );

            if (rawSeasonDataResult.IsFailed)
                return rawSeasonDataResult.ToResult();

            // Phase 3 of 5: Episode data was retrieved successfully.
            var rawEpisodesDataResult = await _plexServiceApi.GetAllEpisodesAsync(
                plexLibrary,
                progress => SendProgress(3, progress.Percentage, progress.TimeRemaining)
            );
            if (rawEpisodesDataResult.IsFailed)
                return rawEpisodesDataResult.ToResult();

            _log.Information("Merging all data received from PlexApi for library {PlexLibraryName}", plexLibrary.Name);

            // Phase 4 of 5: PlexLibrary media data was parsed successfully.
            _log.Here()
                .Debug(
                    "Finished retrieving all media for library {PlexLibraryName} in {Elapsed:000} seconds",
                    plexLibrary.Title,
                    timer.Elapsed.TotalSeconds
                );
            timer.Restart();

            // Phase 4 of 5: PlexLibrary media data was parsed successfully.
            BuildTvShowTree(plexLibrary, rawSeasonDataResult.Value, rawEpisodesDataResult.Value);

            // Update the MetaData of this library
            var updateMetaDataResult = plexLibrary.UpdateMetaData();
            if (updateMetaDataResult.IsFailed)
                return updateMetaDataResult;

            await _dbContext.UpdatePlexLibraryById(plexLibrary);

            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexTvShowsCommand(plexLibrary));
            if (createResult.IsFailed)
                return createResult.ToResult();

            _log.Here()
                .Debug(
                    "Finished updating all media in the database for library {PlexLibraryName} in {Elapsed:000} seconds",
                    plexLibrary.Title,
                    timer.Elapsed.TotalSeconds
                );

            // Phase 5 of 5: Database has been successfully updated with new library data.
            SendProgress(5, 1);
        }

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibrary.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.SyncedAt, plexLibrary.SyncedAt));

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        return Result.Ok(plexLibrary);
    }

    private async Task<Result<PlexLibrary>> RefreshPlexMovieLibrary(PlexLibrary plexLibrary)
    {
        // Phase 2 of 5: Season data was retrieved successfully.
        SendProgress(2, 1);

        // Update the MetaData of this library
        var updateMetaDataResult = plexLibrary.UpdateMetaData();
        if (updateMetaDataResult.IsFailed)
            return updateMetaDataResult;

        // Phase 3 of 5: Meta-data was updated successfully.
        SendProgress(3, 1);

        await _dbContext.UpdatePlexLibraryById(plexLibrary);

        // Phase 4 of 5: PlexLibrary media data was parsed successfully.
        SendProgress(4, 1);

        if (plexLibrary.HasMedia)
        {
            var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
            if (createResult.IsFailed)
            {
                SendProgress(5, 1);
                return createResult;
            }
        }

        // Phase 5 of 5: Movies have been successfully updated in the database.
        SendProgress(5, 1);

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibrary.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.SyncedAt, plexLibrary.SyncedAt));

        return Result.Ok(plexLibrary);
    }

    // Send progress
    private void SendProgress(int step, decimal percentage, TimeSpan timeRemaining = default)
    {
        var countStep = _baseCountProgress / _totalProgressSteps;
        var index = countStep * step + countStep * percentage;

        var progress = new LibraryProgress()
        {
            TimeRemaining = timeRemaining,
            Id = _plexLibraryId,
            Step = step,
            Received = (int)Math.Floor(index),
            Total = _baseCountProgress,
            IsRefreshing = false,
        };

        _progressAction?.Invoke(progress);

        _signalRService.SendLibraryProgressUpdateAsync(progress);
    }

    private void BuildTvShowTree(
        PlexLibrary plexLibrary,
        List<PlexTvShowSeason> rawSeasonDataResult,
        List<PlexTvShowEpisode> rawEpisodesDataResult
    )
    {
        var timer = new Stopwatch();
        timer.Start();

        // Group seasons and episodes by parent key upfront
        var seasonsByTvShowKey = rawSeasonDataResult
            .GroupBy(x => x.ParentKey)
            .ToDictionary(g => g.Key, g => g.ToList());
        var episodesBySeasonKey = rawEpisodesDataResult
            .GroupBy(x => x.ParentKey)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (var i = 0; i < plexLibrary.TvShows.Count; i++)
        {
            var plexTvShow = plexLibrary.TvShows[i];

            // Retrieve and assign seasons for this TV show
            if (seasonsByTvShowKey.TryGetValue(plexTvShow.Key, out var seasons))
            {
                plexTvShow.Seasons = seasons;
                plexTvShow.ChildCount = seasons.Count;

                // Remove seasons that have been assigned
                seasonsByTvShowKey.Remove(plexTvShow.Key);
            }

            foreach (var plexTvShowSeason in plexTvShow.Seasons)
            {
                plexTvShowSeason.PlexLibraryId = plexLibrary.Id;
                plexTvShowSeason.PlexLibrary = plexLibrary;
                plexTvShowSeason.TvShow = plexTvShow;

                // Retrieve and assign episodes for this season
                if (episodesBySeasonKey.TryGetValue(plexTvShowSeason.Key, out var episodes))
                {
                    // Set library ID in each episode
                    episodes.ForEach(x => x.PlexLibraryId = plexLibrary.Id);

                    plexTvShowSeason.Episodes = episodes;
                    plexTvShowSeason.ChildCount = episodes.Count;

                    // Remove episodes that have been assigned
                    episodesBySeasonKey.Remove(plexTvShowSeason.Key);

                    // Set the season's year based on the first episode's year
                    if (plexTvShowSeason.Year == 0 && episodes.Any())
                        plexTvShowSeason.Year = episodes.First().Year;

                    plexTvShowSeason.MediaSize = episodes.Sum(x => x.MediaSize);
                    plexTvShowSeason.Duration = episodes.Sum(x => x.Duration);
                }
            }

            plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
            plexTvShow.Duration = plexTvShow.Seasons.Sum(x => x.Duration);
            SendProgress(4, DataFormat.GetPercentage(i, plexLibrary.TvShows.Count));
        }

        timer.Stop();
        _log.Here()
            .Debug(
                "Finished merging all media for library {PlexLibraryName} in {Elapsed:000} seconds",
                plexLibrary.Title,
                timer.Elapsed.TotalSeconds
            );
    }
}
