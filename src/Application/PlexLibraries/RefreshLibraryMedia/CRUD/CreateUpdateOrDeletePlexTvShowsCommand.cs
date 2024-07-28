using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record CreateUpdateOrDeletePlexTvShowsCommand : IRequest<Result<CrudTvShowsReport>>
{
    public CreateUpdateOrDeletePlexTvShowsCommand(PlexLibrary plexLibrary)
    {
        PlexLibrary = plexLibrary;
    }

    public PlexLibrary PlexLibrary { get; }
}

public class CreateUpdateOrDeletePlexTvShowsCommandValidator : AbstractValidator<CreateUpdateOrDeletePlexTvShowsCommand>
{
    public CreateUpdateOrDeletePlexTvShowsCommandValidator()
    {
        RuleFor(x => x.PlexLibrary).NotNull();
        RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
        RuleFor(x => x.PlexLibrary.Title).NotEmpty();
        RuleFor(x => x.PlexLibrary.TvShows).NotEmpty();

        RuleForEach(x => x.PlexLibrary.TvShows)
            .ChildRules(plexTvShow =>
            {
                plexTvShow.RuleFor(x => x.Key).GreaterThan(0);
                plexTvShow
                    .RuleForEach(x => x.Seasons)
                    .ChildRules(plexTvShowSeason =>
                    {
                        plexTvShowSeason.RuleFor(x => x.Key).GreaterThan(0);
                    });
            });
    }
}

public class CreateUpdateOrDeletePlexTvShowsCommandHandler
    : IRequestHandler<CreateUpdateOrDeletePlexTvShowsCommand, Result<CrudTvShowsReport>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly BulkConfig _config =
        new()
        {
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
            CalculateStats = true,
        };

    private Dictionary<int, int> _tvShowKeyToIdsDict = new();
    private Dictionary<int, int> _seasonKeyToIdsDict = new();
    private Dictionary<int, int> _episodeKeyToIdsDict = new();

    public CreateUpdateOrDeletePlexTvShowsCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<CrudTvShowsReport>> Handle(
        CreateUpdateOrDeletePlexTvShowsCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var report = new CrudTvShowsReport();
            var plexLibrary = command.PlexLibrary;

            _log.Debug(
                "Starting adding, updating or deleting of tv shows in library: {PlexLibraryName} with id:  {PlexLibraryId}",
                plexLibrary.Title,
                plexLibrary.Id
            );

            // Retrieve current library data from the database
            var tvShowsInDb = await _dbContext
                .PlexTvShows.Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync(cancellationToken);
            _tvShowKeyToIdsDict = tvShowsInDb.ToDictionary(x => x.Key, x => x.Id);

            var seasonsInDb = await _dbContext
                .PlexTvShowSeason.Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync(cancellationToken);
            _seasonKeyToIdsDict = seasonsInDb.ToDictionary(x => x.Key, x => x.Id);

            var episodesInDb = await _dbContext
                .PlexTvShowEpisodes.Where(x => x.PlexLibraryId == plexLibrary.Id)
                .ToListAsync(cancellationToken);
            _episodeKeyToIdsDict = episodesInDb.ToDictionary(x => x.Key, x => x.Id);

            // Ensure all tvShows, seasons and episodes have the correct plexLibraryId assigned and other values
            var mediaData = SetIds(plexLibrary, command.PlexLibrary.TvShows);

            var rawTvShows = mediaData.Select(x => x).ToList();
            var rawTvShowsDict = rawTvShows.ToDictionary(x => x.Key);

            var rawSeasons = rawTvShows.SelectMany(x => x.Seasons).ToList();
            var rawSeasonsDict = rawSeasons.ToDictionary(x => x.Key);

            var rawEpisodes = rawSeasons.SelectMany(x => x.Episodes).ToList();
            var rawEpisodesDict = rawEpisodes.ToDictionary(x => x.Key);

            // Create dictionaries on how to update the database.
            var addTvShowList = rawTvShows.Where(x => !_tvShowKeyToIdsDict.ContainsKey(x.Key)).ToList();
            var updateTvShowList = rawTvShows.Where(x => x.Id > 0).ToList();
            var deleteTvShowList = tvShowsInDb.Where(x => !rawTvShowsDict.ContainsKey(x.Key)).ToList();

            // Generate add dictionaries for seasons and episodes
            var addSeasonList = rawSeasons.Where(x => rawTvShowsDict.ContainsKey(x.ParentKey)).ToList();
            var updateSeasonList = rawSeasons.Where(x => x.Id > 0).ToList();
            var deleteSeasonList = seasonsInDb.Where(x => !rawSeasonsDict.ContainsKey(x.Key)).ToList();

            // Generate delete dictionaries for seasons and episodes
            var addEpisodeList = rawEpisodes.Where(x => rawSeasonsDict.ContainsKey(x.ParentKey)).ToList();
            var updateEpisodeList = rawEpisodes.Where(x => x.Id > 0).ToList();
            var deleteEpisodeList = episodesInDb.Where(x => !rawEpisodesDict.ContainsKey(x.Key)).ToList();

            // Add tvShows to DB.
            await _dbContext.BulkInsertAsync(addTvShowList, _config, cancellationToken);
            report.CreatedTvShows = addTvShowList.Count;

            foreach (var tvShow in addTvShowList)
                _tvShowKeyToIdsDict.TryAdd(tvShow.Key, tvShow.Id);

            // Add tvShowId to every season and then insert seasons in DB.
            foreach (var season in addSeasonList)
            {
                if (_tvShowKeyToIdsDict.TryGetValue(season.ParentKey, out var tvShowId))
                    season.TvShowId = tvShowId;

                foreach (var x in addEpisodeList.Where(x => x.ParentKey == season.Key))
                    x.TvShowId = tvShowId;
            }

            await _dbContext.BulkInsertAsync(addSeasonList, _config, cancellationToken);
            report.CreatedSeasons = addSeasonList.Count;

            foreach (var season in addSeasonList)
                _seasonKeyToIdsDict.TryAdd(season.Key, season.Id);

            // Add tvShow id and season id to every episode and then insert episodes in DB.
            foreach (var episode in addEpisodeList)
                if (_seasonKeyToIdsDict.TryGetValue(episode.ParentKey, out var seasonId))
                    episode.TvShowSeasonId = seasonId;

            await _dbContext.BulkInsertAsync(addEpisodeList, _config, cancellationToken);
            report.CreatedEpisodes = addEpisodeList.Count;

            foreach (var episode in addEpisodeList)
                _episodeKeyToIdsDict.TryAdd(episode.Key, episode.Id);

            // Update existing entries
            await _dbContext.BulkUpdateAsync(updateTvShowList, _config, cancellationToken);
            report.UpdatedTvShows = updateTvShowList.Count;

            await _dbContext.BulkUpdateAsync(updateSeasonList, _config, cancellationToken);
            report.UpdatedSeasons = updateSeasonList.Count;

            await _dbContext.BulkUpdateAsync(updateEpisodeList, _config, cancellationToken);
            report.UpdatedEpisodes = updateEpisodeList.Count;

            // Delete missing tvShow entries
            var deleteEpisodeIds = deleteEpisodeList.Select(x => x.Id).ToList();
            report.DeletedEpisodes = await _dbContext
                .PlexTvShowEpisodes.Where(x => deleteEpisodeIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);

            var deleteSeasonIds = deleteSeasonList.Select(x => x.Id).ToList();
            report.DeletedSeasons = await _dbContext
                .PlexTvShowSeason.Where(x => deleteSeasonIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);

            var deleteTvShowIds = deleteTvShowList.Select(x => x.Id).ToList();
            report.DeletedTvShows = await _dbContext
                .PlexTvShows.Where(x => deleteTvShowIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);

            stopWatch.Stop();
            _log.Information(
                "Finished updating plexLibrary: {PlexLibraryName} with id: {PlexLibraryId} in {TotalSeconds} seconds",
                plexLibrary.Title,
                plexLibrary.Id,
                stopWatch.Elapsed.TotalSeconds
            );
            _log.DebugLine(report.ToString());
            return Result.Ok(report);
        }
        catch (Exception ex)
        {
            return Result
                .Fail(new ExceptionalError("An error occurred while processing the Plex TV shows.", ex))
                .LogError();
        }
    }

    private List<PlexTvShow> SetIds(PlexLibrary plexLibrary, List<PlexTvShow> plexTvShows)
    {
        foreach (var plexTvShow in plexTvShows)
        {
            if (_tvShowKeyToIdsDict.TryGetValue(plexTvShow.Key, out var tvShowId))
                plexTvShow.Id = tvShowId;

            plexTvShow.PlexLibraryId = plexLibrary.Id;
            plexTvShow.PlexServerId = plexLibrary.PlexServerId;

            foreach (var season in plexTvShow.Seasons)
            {
                if (_seasonKeyToIdsDict.TryGetValue(season.Key, out var seasonId))
                    season.Id = seasonId;

                season.TvShowId = plexTvShow.Id;
                season.ParentKey = plexTvShow.Key;

                season.PlexLibraryId = plexLibrary.Id;
                season.PlexServerId = plexLibrary.PlexServerId;

                foreach (var episode in season.Episodes)
                {
                    if (_episodeKeyToIdsDict.TryGetValue(episode.Key, out var episodeId))
                        episode.Id = episodeId;

                    episode.TvShowId = plexTvShow.Id;
                    episode.TvShowSeasonId = season.Id;
                    episode.ParentKey = season.Key;

                    episode.PlexLibraryId = plexLibrary.Id;
                    episode.PlexServerId = plexLibrary.PlexServerId;
                }
            }
        }

        return plexTvShows;
    }
}

public record CrudTvShowsReport
{
    public int CreatedTvShows { get; set; }

    public int UpdatedTvShows { get; set; }

    public int DeletedTvShows { get; set; }

    public int CreatedSeasons { get; set; }

    public int UpdatedSeasons { get; set; }

    public int DeletedSeasons { get; set; }

    public int CreatedEpisodes { get; set; }

    public int UpdatedEpisodes { get; set; }

    public int DeletedEpisodes { get; set; }

    public override string ToString() =>
        $@"
        CreatedTvShows: {CreatedTvShows}
        UpdatedTvShows: {UpdatedTvShows}
        DeletedTvShows: {DeletedTvShows}
        CreatedSeasons: {CreatedSeasons}
        UpdatedSeasons: {UpdatedSeasons}
        DeletedSeasons: {DeletedSeasons}
        CreatedEpisodes: {CreatedEpisodes}
        UpdatedEpisodes: {UpdatedEpisodes}
        DeletedEpisodes: {DeletedEpisodes}";
}
