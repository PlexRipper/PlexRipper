using FluentResults;
using Reaparr.Domain;
using Reaparr.Logging;

namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    /// <summary>
    /// Bulk inserts the Plex TV shows and their hierarchical media data (seasons, episodes, media data, parts, and streams) into the database.
    /// </summary>
    public static async Task<Result<BulkInsertTvShowsRapport>> BulkInsertPlexTvShowsAsync(
        this IReaparrDbContext context,
        List<PlexTvShow> plexTvShows,
        int plexServerId,
        int plexLibraryId,
        CancellationToken ct = default
    )
    {
        try
        {
            if (!plexTvShows.Any())
                return Result.Fail("No tv-shows to insert").LogWarning();

            if (plexServerId == 0)
                return ResultExtensions.IsZero(nameof(plexServerId));

            if (plexLibraryId == 0)
                return ResultExtensions.IsZero(nameof(plexLibraryId));

            var rapport = new BulkInsertTvShowsRapport();

            plexTvShows.SetRelationshipIds(plexServerId, plexLibraryId);

            await context.BulkInsertAsync(plexTvShows, BulkConfigPreset.Default, ct);
            rapport.CreatedTvShows = plexTvShows.Count;

            // Insert seasons for each TV show
            var seasons = plexTvShows
                .SelectMany(tvShow =>
                {
                    tvShow.Seasons.SetRelationshipIds(tvShow.PlexServerId, tvShow.PlexLibraryId, tvShow.Id);
                    return tvShow.Seasons;
                })
                .ToList();

            await context.BulkInsertAsync(seasons, BulkConfigPreset.Default, ct);
            rapport.CreatedSeasons = seasons.Count;

            // Insert episodes for each season
            var episodes = seasons
                .SelectMany(season =>
                {
                    season.Episodes.SetRelationshipIds(
                        season.PlexServerId,
                        season.PlexLibraryId,
                        season.TvShowId,
                        season.Id
                    );

                    foreach (var episode in season.Episodes)
                        episode.TvShowSeason = season;

                    return season.Episodes;
                })
                .ToList();

            await context.BulkInsertAsync(episodes, BulkConfigPreset.Default, ct);
            rapport.CreatedEpisodes = episodes.Count;

            // Insert media data for each episode
            var mediaData = episodes
                .SelectMany(episode =>
                {
                    episode.MediaDataList.SetRelationshipIds(episode.PlexServerId, episode.PlexLibraryId, episode.Id);

                    foreach (var episodeMediaData in episode.MediaDataList)
                        episodeMediaData.PlexTvShowEpisode = episode;

                    return episode.MediaDataList;
                })
                .ToList();

            await context.BulkInsertAsync(mediaData, BulkConfigPreset.Default, ct);

            // Add TvShowSeason Qualities for each tv-show
            var seasonQualities = mediaData
                .Select(x => new PlexTvShowSeasonMediaQuality
                {
                    Id = 0,
                    Quality = x.Quality,
                    PlexLibraryId = x.PlexLibraryId,
                    PlexTvShowSeasonId = x.PlexTvShowEpisode?.TvShowSeasonId ?? 0,
                    PlexTvShowSeason = x.PlexTvShowEpisode?.TvShowSeason,
                })
                .DistinctBy(x => (x.Quality, x.PlexTvShowSeasonId))
                .ToList();
            await context.BulkInsertAsync(seasonQualities, BulkConfigPreset.Default, ct);

            // Add TvShow Qualities for each tv-show
            var tvShowQualities = seasonQualities
                .Select(x => new PlexTvShowMediaQuality
                {
                    Id = 0,
                    Quality = x.Quality,
                    PlexLibraryId = x.PlexLibraryId,
                    PlexTvShowId = x.PlexTvShowSeason?.TvShowId ?? 0,
                })
                .DistinctBy(x => (x.Quality, x.PlexTvShowId))
                .ToList();
            await context.BulkInsertAsync(tvShowQualities, BulkConfigPreset.Default, ct);

            return Result.Ok(rapport);
        }
        catch (Exception e)
        {
            var serverName = await context.GetPlexServerNameById(plexServerId);
            var libraryName = await context.GetPlexLibraryNameById(plexLibraryId);
            _log.Here()
                .Error(
                    "Error while bulk inserting plex tv-shows in Plex server: {PlexServerName} with serverId: {PlexServerId} and Plex library {PlexLibraryName} libraryId: {PlexLibraryId}",
                    serverName,
                    plexServerId,
                    libraryName,
                    plexLibraryId
                );
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
