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

        var rapport = new BulkInsertTvShowsRapport();

        plexTvShows.SetRelationshipIds(plexServerId, plexLibraryId);

        // Phase 1: Insert TV shows
        var insertTvShowsResult = await Result.Try(async Task () =>
        {
            await context.BulkInsertAsync(plexTvShows, ct);
            rapport.CreatedTvShows = plexTvShows.Count;
        });

        if (insertTvShowsResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertTvShowsResult, ct);

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

        var insertSeasonsResult = await Result.Try(async Task () =>
        {
            await context.BulkInsertAsync(seasonsToInsert, ct);
            rapport.CreatedSeasons = seasonsToInsert.Count;
        });

        if (insertSeasonsResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertSeasonsResult, ct);

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

        var insertEpisodesResult = await Result.Try(async Task () =>
        {
            await context.BulkInsertAsync(episodesToInsert, ct);
            rapport.CreatedEpisodes = episodesToInsert.Count;
        });

        if (insertEpisodesResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertEpisodesResult, ct);

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

        var insertMediaDataResult = await Result.Try(async Task () =>
            await context.BulkInsertAsync(mediaData, ct)
        );

        if (insertMediaDataResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertMediaDataResult, ct);

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

        var insertSeasonQualitiesResult = await Result.Try(async Task () =>
            await context.BulkInsertAsync(seasonQualities, ct)
        );

        if (insertSeasonQualitiesResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertSeasonQualitiesResult, ct);

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

        var insertTvShowQualitiesResult = await Result.Try(async Task () =>
            await context.BulkInsertAsync(tvShowQualities, ct)
        );

        if (insertTvShowQualitiesResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertTvShowQualitiesResult, ct);

        foreach (var tvShow in plexTvShows)
        {
            var highestQuality = tvShowQualities
                .Where(x => x.PlexTvShowId == tvShow.Id)
                .Select(x => (VideoQuality?)x.Quality)
                .Max();

            tvShow.Quality = highestQuality ?? VideoQuality.Unknown;
        }

        var updateTvShowQualityResult = await Result.Try(async Task () =>
            await context.BulkUpdateAsync(plexTvShows, BulkConfigPreset.Default, ct)
        );

        if (updateTvShowQualityResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, updateTvShowQualityResult, ct);

        return Result.Ok(rapport);
    }

    private static async Task<Result<BulkInsertTvShowsRapport>> RollbackTvShowInsertAsync(
        IReaparrDbContext context,
        int plexLibraryId,
        Result failedResult,
        CancellationToken ct
    )
    {
        _log.Here()
            .Warning(
                "Rolling back partial TV show insert for PlexLibraryId: {PlexLibraryId} due to: {Errors}",
                plexLibraryId,
                failedResult.Errors
            );

        await Result.Try(async Task () =>
        {
            await context.PlexTvShowMediaQualities.Where(x => x.PlexLibraryId == plexLibraryId).ExecuteDeleteAsync(ct);
            await context
                .PlexTvShowSeasonMediaQualities.Where(x => x.PlexLibraryId == plexLibraryId)
                .ExecuteDeleteAsync(ct);
            await context.PlexTvShowEpisodeData.Where(x => x.PlexLibraryId == plexLibraryId).ExecuteDeleteAsync(ct);
            await context.PlexTvShowEpisodes.Where(x => x.PlexLibraryId == plexLibraryId).ExecuteDeleteAsync(ct);
            await context.PlexTvShowSeason.Where(x => x.PlexLibraryId == plexLibraryId).ExecuteDeleteAsync(ct);
            await context.PlexTvShows.Where(x => x.PlexLibraryId == plexLibraryId).ExecuteDeleteAsync(ct);
        });

        return failedResult;
    }
}
