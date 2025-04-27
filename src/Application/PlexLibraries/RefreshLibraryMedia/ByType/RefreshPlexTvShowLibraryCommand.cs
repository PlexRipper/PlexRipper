using System.Diagnostics;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public record RefreshPlexTvShowLibraryCommand(PlexLibrary PlexLibrary, Action<LibraryProgress> Action)
    : ICommand<Result<PlexLibrary>>;

public class RefreshPlexTvShowLibraryCommandValidator : AbstractValidator<RefreshPlexTvShowLibraryCommand>
{
    public RefreshPlexTvShowLibraryCommandValidator()
    {
        RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
    }
}

public class RefreshPlexTvShowLibraryCommandHandler
    : ICommandHandler<RefreshPlexTvShowLibraryCommand, Result<PlexLibrary>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandDispatch _commandDispatch;
    private readonly IRefreshLibraryProgressReporter _progressReporter;

    public RefreshPlexTvShowLibraryCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        ICommandDispatch commandDispatch,
        IRefreshLibraryProgressReporter progressReporter
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _commandDispatch = commandDispatch;
        _progressReporter = progressReporter;
    }

    public async Task<Result<PlexLibrary>> ExecuteAsync(
        RefreshPlexTvShowLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexLibrary = command.PlexLibrary;

        if (plexLibrary.Type != PlexMediaType.TvShow)
            return Result.Fail("PlexLibrary is not of type TvShow").LogError();

        if (plexLibrary.TvShows.Any())
        {
            var timer = new Stopwatch();
            timer.Start();

            // Phase 2 of 5: Season data was retrieved successfully.
            var rawSeasonDataResult = await _commandDispatch.ExecuteAsync(
                new GetAllMediaSeasonsCommand(
                    plexLibrary,
                    progress => _progressReporter.SendProgress(new RefreshLibraryProgressUpdate()
                    {
                        Action = command.Action,
                        PlexLibraryType = PlexMediaType.TvShow,
                        PlexLibraryId = plexLibrary.Id,
                        Step = 2,
                        Percentage = progress.Percentage,
                        TimeRemaining = progress.TimeRemaining,
                    })
                ),
                cancellationToken
            );

            if (rawSeasonDataResult.IsFailed)
                return rawSeasonDataResult.ToResult();

            // Phase 3 of 5: Episode data was retrieved successfully.
            var rawEpisodesDataResult = await _commandDispatch.ExecuteAsync(
                new GetAllMediaEpisodesCommand(
                    plexLibrary,
                    progress => _progressReporter.SendProgress(new RefreshLibraryProgressUpdate()
                    {
                        Action = command.Action,
                        PlexLibraryType = PlexMediaType.TvShow,
                        PlexLibraryId = plexLibrary.Id,
                        Step = 3,
                        Percentage = progress.Percentage,
                        TimeRemaining = progress.TimeRemaining,
                    })
                ),
                cancellationToken
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
            _progressReporter.SendProgress(new RefreshLibraryProgressUpdate()
            {
                Action = command.Action,
                PlexLibraryType = PlexMediaType.TvShow,
                PlexLibraryId = plexLibrary.Id,
                Step = 4,
                Percentage = 1,
            });

            // Update the MetaData of this library
            var syncResult = await _mediator.Send(new SyncPlexTvShowsCommand(tvShows), cancellationToken);
            if (syncResult.IsFailed)
            {
                _progressReporter.SendProgress(new RefreshLibraryProgressUpdate()
                {
                    Action = command.Action,
                    PlexLibraryType = PlexMediaType.TvShow,
                    PlexLibraryId = plexLibrary.Id,
                    Step = 5,
                    Percentage = 1,
                });

                return syncResult.ToResult().LogError();
            }

            var mediaSize = tvShows.Sum(x => x.MediaSize);
            plexLibrary.SetTvShowMetaData(
                plexLibrary.TvShows.Count,
                rawSeasonData.Count,
                rawEpisodesData.Count,
                mediaSize
            );

            if (plexLibrary.TvShows.Any() && mediaSize == 0)
            {
                _log.Error(
                    "No media size was found for library {PlexLibraryName} with id: {PlexLibraryId}",
                    plexLibrary.Title,
                    plexLibrary.Id
                );
            }

            await _dbContext.UpdatePlexLibraryById(plexLibrary, CancellationToken.None);

            _log.Here()
                .Debug(
                    "Finished updating all media in the database for library {PlexLibraryName} in {Elapsed:0} seconds",
                    plexLibrary.Title,
                    timer.Elapsed.TotalSeconds
                );

            // Phase 5 of 5: Database has been successfully updated with new library data.
            _progressReporter.SendProgress(new RefreshLibraryProgressUpdate
            {
                Action = command.Action,
                PlexLibraryType = PlexMediaType.TvShow,
                PlexLibraryId = plexLibrary.Id,
                Step = 5,
                Percentage = 1,
            });
        }
        else
        {
            _log.Warning(
                "No TV shows were found for library {PlexLibraryName} with id: {PlexLibraryId}",
                plexLibrary.Title,
                plexLibrary.Id
            );
        }

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibrary.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.SyncedAt, plexLibrary.SyncedAt), CancellationToken.None);

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        return Result.Ok(plexLibrary);
    }

    private List<PlexTvShow> BuildTvShowTree(
        PlexLibrary plexLibrary,
        List<PlexTvShow> rawTvShowData,
        List<PlexTvShowSeason> rawSeasonData,
        List<PlexTvShowEpisode> rawEpisodesData
    )
    {
        var (validSeasons, validEpisodes) = Filter(rawSeasonData, rawEpisodesData, plexLibrary);

        // Group seasons and episodes by parent key upfront
        var seasonsByTvShowKey = validSeasons.GroupBy(x => x.ParentGuid!).ToDictionary(g => g.Key, g => g.ToList());
        var episodesBySeasonKey = validEpisodes.GroupBy(x => x.ParentGuid!).ToDictionary(g => g.Key, g => g.ToList());

        for (var i = 0; i < rawTvShowData.Count; i++)
        {
            var plexTvShow = rawTvShowData[i];
            plexTvShow.PlexLibraryId = plexLibrary.Id;
            plexTvShow.PlexServerId = plexLibrary.PlexServerId;
            plexTvShow.SortIndex = i + 1;

            // Retrieve and assign seasons for this TV show
            if (seasonsByTvShowKey.TryGetValue(plexTvShow.Guid, out var seasons))
            {
                plexTvShow.Seasons = seasons;
                plexTvShow.ChildCount = seasons.Count;

                // Remove seasons that have been assigned
                seasonsByTvShowKey.Remove(plexTvShow.Guid);
            }

            for (var seasonIndex = 0; seasonIndex < plexTvShow.Seasons.Count; seasonIndex++)
            {
                var plexTvShowSeason = plexTvShow.Seasons[seasonIndex];
                plexTvShowSeason.SortIndex = seasonIndex + 1;
                plexTvShowSeason.PlexLibraryId = plexLibrary.Id;
                plexTvShowSeason.PlexServerId = plexLibrary.PlexServerId;
                plexTvShowSeason.TvShow = plexTvShow;

                // Retrieve and assign episodes for this season
                if (!episodesBySeasonKey.TryGetValue(plexTvShowSeason.Guid, out var episodes))
                    continue;

                // Set library ID in each episode
                var episodeIndex = 1;
                episodes.ForEach((x) =>
                    {
                        x.PlexLibraryId = plexLibrary.Id;
                        x.PlexServerId = plexLibrary.PlexServerId;
                        x.SortIndex = episodeIndex++;
                    }
                );

                plexTvShowSeason.Episodes = episodes;
                plexTvShowSeason.ChildCount = episodes.Count;

                // Remove episodes that have been assigned
                episodesBySeasonKey.Remove(plexTvShowSeason.Guid);

                // Set the season's year based on the first episode's year
                if (plexTvShowSeason.Year == 0 && episodes.Any())
                    plexTvShowSeason.Year = episodes.First().Year;

                plexTvShowSeason.MediaSize = episodes.Sum(x => x.MediaSize);
                plexTvShowSeason.Duration = episodes.Sum(x => x.Duration);
            }

            plexTvShow.MediaSize = plexTvShow.Seasons.Sum(x => x.MediaSize);
            plexTvShow.Duration = plexTvShow.Seasons.Sum(x => x.Duration);
            plexTvShow.GrandChildCount = plexTvShow.Seasons.Sum(x => x.ChildCount);
        }

        return rawTvShowData;
    }

    private (List<PlexTvShowSeason> validSeasons, List<PlexTvShowEpisode> validEpisodes) Filter(
        List<PlexTvShowSeason> rawSeasonData,
        List<PlexTvShowEpisode> rawEpisodesData,
        PlexLibrary library
    )
    {
        var validSeasons = new List<PlexTvShowSeason>();
        var inValidSeasons = new List<PlexTvShowSeason>();

        var validEpisodes = new List<PlexTvShowEpisode>();
        var inValidEpisodes = new List<PlexTvShowEpisode>();

        foreach (var plexTvShowSeason in rawSeasonData)
        {
            if (plexTvShowSeason.ParentGuid != null)
            {
                validSeasons.Add(plexTvShowSeason);
                continue;
            }

            inValidSeasons.Add(plexTvShowSeason);
        }

        foreach (var episode in rawEpisodesData)
        {
            if (episode.ParentGuid != null)
            {
                validEpisodes.Add(episode);
                continue;
            }

            inValidEpisodes.Add(episode);
        }

        // Log invalid seasons and episodes
        if (inValidSeasons.Any())
        {
            _log.Warning(
                "Found {Count} invalid seasons which are missing a ParentGUID in library {PlexLibraryName} with id: {PlexLibraryId}",
                inValidSeasons.Count,
                library.Title,
                library.Id
            );
        }

        if (inValidEpisodes.Any())
        {
            _log.Warning(
                "Found {Count} invalid episodes which are missing a ParentGUID in library {PlexLibraryName} with id: {PlexLibraryId}",
                inValidEpisodes.Count,
                library.Title,
                library.Id
            );
        }

        return (validSeasons, validEpisodes);
    }
}