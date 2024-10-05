using System.Diagnostics;
using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
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

public class RefreshLibraryMediaCommandValidator : AbstractValidator<RefreshLibraryMediaCommand>
{
    public RefreshLibraryMediaCommandValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaCommandHandler : IRequestHandler<RefreshLibraryMediaCommand, Result<PlexLibrary>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;
    private readonly IPlexApiService _plexServiceApi;

    private readonly int _baseCountProgress = 1000;
    private int _totalProgressSteps;

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

        _totalProgressSteps = plexLibrary.Type switch
        {
            PlexMediaType.TvShow => 5,
            PlexMediaType.Movie => 3,
            _ => _totalProgressSteps,
        };

        // Phase 1:  Retrieve overview of all media belonging to this PlexLibrary
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

            var rawSeasonData = rawSeasonDataResult.Value;
            var rawEpisodesData = rawEpisodesDataResult.Value;

            // Phase 4 of 5: PlexLibrary media data was parsed successfully.
            var tvShows = BuildTvShowTree(plexLibrary, plexLibrary.TvShows, rawSeasonData, rawEpisodesData);

            // Update the MetaData of this library
            var syncResult = await _mediator.Send(new SyncPlexTvShowsCommand(tvShows));
            if (syncResult.IsFailed)
            {
                SendProgress(5, 1);
                return syncResult.ToResult().LogError();
            }

            plexLibrary.MetaData = new PlexLibraryMetaData()
            {
                MovieCount = 0,
                TvShowCount = plexLibrary.TvShows.Count,
                TvShowSeasonCount = rawSeasonData.Count,
                TvShowEpisodeCount = rawEpisodesData.Count,
                MediaSize = tvShows.Sum(x => x.MediaSize),
            };

            await _dbContext.UpdatePlexLibraryById(plexLibrary);

            _log.Here()
                .Debug(
                    "Finished updating all media in the database for library {PlexLibraryName} in {Elapsed:0} seconds",
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
        if (plexLibrary.Movies.Any())
        {
            foreach (var plexTvShow in plexLibrary.Movies)
            {
                plexTvShow.PlexLibraryId = plexLibrary.Id;
                plexTvShow.PlexServerId = plexLibrary.PlexServerId;
            }

            var createResult = await _mediator.Send(new SyncPlexMoviesCommand(plexLibrary.Movies));
            if (createResult.IsFailed)
            {
                SendProgress(_totalProgressSteps, 1);
                return createResult.ToResult().LogError();
            }
        }

        // Phase 2 of 3: PlexLibrary media data was parsed successfully.
        SendProgress(2, 1);

        plexLibrary.MetaData = new PlexLibraryMetaData()
        {
            MovieCount = plexLibrary.Movies.Count,
            TvShowCount = 0,
            TvShowSeasonCount = 0,
            TvShowEpisodeCount = 0,
            MediaSize = plexLibrary.Movies.Sum(x => x.MediaSize),
        };

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;

        await _dbContext.UpdatePlexLibraryById(plexLibrary);

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        // Phase 3 of 3: Movies have been successfully updated in the database.
        SendProgress(_totalProgressSteps, 1);

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
            TotalSteps = _totalProgressSteps,
        };

        _progressAction?.Invoke(progress);

        _signalRService.SendLibraryProgressUpdateAsync(progress);
    }

    private List<PlexTvShow> BuildTvShowTree(
        PlexLibrary plexLibrary,
        List<PlexTvShow> rawTvShowData,
        List<PlexTvShowSeason> rawSeasonData,
        List<PlexTvShowEpisode> rawEpisodesData
    )
    {
        // Group seasons and episodes by parent key upfront
        var seasonsByTvShowKey = rawSeasonData.GroupBy(x => x.ParentKey).ToDictionary(g => g.Key, g => g.ToList());
        var episodesBySeasonKey = rawEpisodesData.GroupBy(x => x.ParentKey).ToDictionary(g => g.Key, g => g.ToList());

        for (var i = 0; i < rawTvShowData.Count; i++)
        {
            var plexTvShow = rawTvShowData[i];
            plexTvShow.PlexLibraryId = plexLibrary.Id;
            plexTvShow.PlexServerId = plexLibrary.PlexServerId;

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
                plexTvShowSeason.PlexServerId = plexLibrary.PlexServerId;
                plexTvShowSeason.TvShow = plexTvShow;

                // Retrieve and assign episodes for this season
                if (episodesBySeasonKey.TryGetValue(plexTvShowSeason.Key, out var episodes))
                {
                    // Set library ID in each episode
                    episodes ??= [];
                    episodes.ForEach(x =>
                    {
                        x.PlexLibraryId = plexLibrary.Id;
                        x.PlexServerId = plexLibrary.PlexServerId;
                    });

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
            SendProgress(4, DataFormat.GetPercentage(i, rawTvShowData.Count));
        }

        return rawTvShowData;
    }
}
