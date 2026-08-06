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
        if (!plexTvShows.Any())
            return Result.Fail("No tv-shows to insert").LogWarning();

        if (plexServerId == 0)
            return ResultExtensions.IsZero(nameof(plexServerId));

        if (plexLibraryId == 0)
            return ResultExtensions.IsZero(nameof(plexLibraryId));

        try
        {
            var rapport = await context.ExecuteSerializedTransactionAsync(async (_, txCt) =>
            {
                var result = new BulkInsertTvShowsRapport();

                plexTvShows.SetRelationshipIds(plexServerId, plexLibraryId);

                // Phase 1: Insert TV shows
                await context.BulkInsertAsync(plexTvShows, BulkConfigPreset.Default, txCt);
                result.CreatedTvShows = plexTvShows.Count;

                // Phase 2: Insert seasons
                var seasons = plexTvShows
                    .SelectMany(tvShow => tvShow.Seasons.Select(season => new { tvShow, season }))
                    .ToList();

                foreach (var entry in seasons)
                {
                    entry.season.PlexServerId = entry.tvShow.PlexServerId;
                    entry.season.PlexLibraryId = entry.tvShow.PlexLibraryId;
                    entry.season.TvShowId = entry.tvShow.Id;
                }

                var seasonsToInsert = seasons.Select(x => x.season).ToList();
                await context.BulkInsertAsync(seasonsToInsert, BulkConfigPreset.Default, txCt);
                result.CreatedSeasons = seasonsToInsert.Count;

                // Phase 3: Insert episodes
                var episodes = seasonsToInsert
                    .SelectMany(season => season.Episodes.Select(episode => new { season, episode }))
                    .ToList();

                foreach (var entry in episodes)
                {
                    entry.episode.PlexServerId = entry.season.PlexServerId;
                    entry.episode.PlexLibraryId = entry.season.PlexLibraryId;
                    entry.episode.TvShowId = entry.season.TvShowId;
                    entry.episode.TvShowSeasonId = entry.season.Id;
                    entry.episode.TvShowSeason = entry.season;
                }

                var episodesToInsert = episodes.Select(x => x.episode).ToList();
                await context.BulkInsertAsync(episodesToInsert, BulkConfigPreset.Default, txCt);
                result.CreatedEpisodes = episodesToInsert.Count;

                // Phase 4: Insert media data
                var mediaData = episodesToInsert
                    .SelectMany(episode =>
                    {
                        episode.MediaDataList.SetRelationshipIds(episode.PlexServerId, episode.PlexLibraryId, episode.Id);

                        foreach (var episodeMediaData in episode.MediaDataList)
                            episodeMediaData.PlexTvShowEpisode = episode;

                        return episode.MediaDataList;
                    })
                    .ToList();

                await context.BulkInsertAsync(mediaData, BulkConfigPreset.Default, txCt);

                // Phase 5: Insert season qualities
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

                await context.BulkInsertAsync(seasonQualities, BulkConfigPreset.Default, txCt);

                // Phase 6: Insert TV show qualities
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

                await context.BulkInsertAsync(tvShowQualities, BulkConfigPreset.Default, txCt);

                foreach (var tvShow in plexTvShows)
                {
                    var highestQuality = tvShowQualities
                        .Where(x => x.PlexTvShowId == tvShow.Id)
                        .Select(x => (VideoQuality?)x.Quality)
                        .Max();

                    tvShow.Quality = highestQuality ?? VideoQuality.Unknown;
                }

                await context.BulkUpdateAsync(plexTvShows, BulkConfigPreset.Default, txCt);

                return result;
            }, ct);

            return Result.Ok(rapport);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }
}
