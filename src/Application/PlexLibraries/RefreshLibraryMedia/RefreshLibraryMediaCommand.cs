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
            progress => SendProgress(1, progress.Percentage),
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

        if (plexLibrary.TvShows.Count == 0)
        {
            return Result
                .Fail(
                    $"PlexLibrary {plexLibrary.Name} with id {plexLibrary.Id} does not contain any TvShows and thus cannot request the corresponding media"
                )
                .LogError();
        }

        var timer = new Stopwatch();
        timer.Start();

        // Phase 2 of 5: Season data was retrieved successfully.
        var rawSeasonDataResult = await _plexServiceApi.GetAllSeasonsAsync(
            plexLibrary,
            progress => SendProgress(2, progress.Percentage)
        );

        if (rawSeasonDataResult.IsFailed)
            return rawSeasonDataResult.ToResult();

        // Phase 3 of 5: Episode data was retrieved successfully.
        var rawEpisodesDataResult = await _plexServiceApi.GetAllEpisodesAsync(
            plexLibrary,
            progress => SendProgress(3, progress.Percentage)
        );
        if (rawEpisodesDataResult.IsFailed)
            return rawEpisodesDataResult.ToResult();

        foreach (var plexTvShow in plexLibrary.TvShows)
        {
            plexTvShow.Seasons = rawSeasonDataResult.Value.FindAll(x => x.ParentKey == plexTvShow.Key);
            plexTvShow.ChildCount = plexTvShow.Seasons.Count;

            foreach (var plexTvShowSeason in plexTvShow.Seasons)
            {
                plexTvShowSeason.PlexLibraryId = plexLibrary.Id;
                plexTvShowSeason.PlexLibrary = plexLibrary;
                plexTvShowSeason.TvShow = plexTvShow;
                plexTvShowSeason.Episodes = rawEpisodesDataResult.Value.FindAll(x =>
                    x.ParentKey == plexTvShowSeason.Key
                );
                plexTvShowSeason.ChildCount = plexTvShowSeason.Episodes.Count;

                // Assume the season started on the year of the first episode
                if (plexTvShowSeason.Year == 0 && plexTvShowSeason.Episodes.Any())
                    plexTvShowSeason.Year = plexTvShowSeason.Episodes.First().Year;

                // Set libraryId in each episode
                plexTvShowSeason.Episodes.ForEach(x => x.PlexLibraryId = plexLibrary.Id);
                plexTvShowSeason.MediaSize = plexTvShowSeason.Episodes.Sum(x => x.MediaSize);
                plexTvShowSeason.Duration = plexTvShowSeason.Episodes.Sum(x => x.Duration);
            }

            plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
            plexTvShow.Duration = plexTvShow.Seasons.Sum(x => x.Duration);
        }

        // Phase 4 of 5: PlexLibrary media data was parsed successfully.
        SendProgress(4, 1);
        _log.Here()
            .Debug(
                "Finished retrieving all media for library {PlexLibraryName} in {Elapsed:000} seconds",
                plexLibrary.Title,
                timer.Elapsed.TotalSeconds
            );
        timer.Restart();

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

        var createResult = await _mediator.Send(new CreateUpdateOrDeletePlexMoviesCommand(plexLibrary));
        if (createResult.IsFailed)
            return createResult;

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
    private void SendProgress(int step, decimal percentage)
    {
        var countStep = _baseCountProgress / _totalProgressSteps;
        var index = countStep * step + countStep * percentage;

        var progress = new LibraryProgress(_plexLibraryId, (int)Math.Floor(index), _baseCountProgress);

        _progressAction?.Invoke(progress);

        _signalRService.SendLibraryProgressUpdateAsync(progress);
    }
}
