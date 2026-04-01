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
            await context.BulkInsertAsync(plexTvShows, BulkConfigPreset.Default, ct);
            rapport.CreatedTvShows = plexTvShows.Count;
        });

        if (insertTvShowsResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertTvShowsResult, ct);

        // Phase 2: Insert seasons
        var seasons = plexTvShows
            .SelectMany(tvShow =>
            {
                tvShow.Seasons.SetRelationshipIds(tvShow.PlexServerId, tvShow.PlexLibraryId, tvShow.Id);
                return tvShow.Seasons;
            })
            .ToList();

        var insertSeasonsResult = await Result.Try(async Task () =>
        {
            await context.BulkInsertAsync(seasons, BulkConfigPreset.Default, ct);
            rapport.CreatedSeasons = seasons.Count;
        });

        if (insertSeasonsResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertSeasonsResult, ct);

        // Phase 3: Insert episodes
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

        var insertEpisodesResult = await Result.Try(async Task () =>
        {
            await context.BulkInsertAsync(episodes, BulkConfigPreset.Default, ct);
            rapport.CreatedEpisodes = episodes.Count;
        });

        if (insertEpisodesResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertEpisodesResult, ct);

        // Phase 4: Insert media data
        var mediaData = episodes
            .SelectMany(episode =>
            {
                episode.MediaDataList.SetRelationshipIds(episode.PlexServerId, episode.PlexLibraryId, episode.Id);

                foreach (var episodeMediaData in episode.MediaDataList)
                    episodeMediaData.PlexTvShowEpisode = episode;

                return episode.MediaDataList;
            })
            .ToList();

        var insertMediaDataResult = await Result.Try(async Task () =>
            await context.BulkInsertAsync(mediaData, BulkConfigPreset.Default, ct)
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
            await context.BulkInsertAsync(seasonQualities, BulkConfigPreset.Default, ct)
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
            await context.BulkInsertAsync(tvShowQualities, BulkConfigPreset.Default, ct)
        );

        if (insertTvShowQualitiesResult.IsFailed)
            return await RollbackTvShowInsertAsync(context, plexLibraryId, insertTvShowQualitiesResult, ct);

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
