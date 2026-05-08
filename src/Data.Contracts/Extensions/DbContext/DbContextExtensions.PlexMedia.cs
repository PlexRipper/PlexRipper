namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<int>> GetPlexMediaByRatingKeyAsync(
        this IReaparrDbContext dbContext,
        int plexApiRatingKey,
        int plexServerId,
        PlexMediaType mediaType,
        CancellationToken cancellationToken = default
    )
    {
        switch (mediaType)
        {
            case PlexMediaType.Movie:
            {
                var entity = await dbContext.PlexMovies.FirstOrDefaultAsync(
                    x => x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId,
                    cancellationToken
                );
                if (entity is not null)
                    return Result.Ok(entity.Id);

                break;
            }
            case PlexMediaType.TvShow:
            {
                var entity = await dbContext.PlexTvShows.FirstOrDefaultAsync(
                    x => x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId,
                    cancellationToken
                );
                if (entity is not null)
                    return Result.Ok(entity.Id);

                break;
            }
            case PlexMediaType.Season:
            {
                var entity = await dbContext.PlexTvShowSeason.FirstOrDefaultAsync(
                    x => x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId,
                    cancellationToken
                );
                if (entity is not null)
                    return Result.Ok(entity.Id);

                break;
            }
            case PlexMediaType.Episode:
            {
                var entity = await dbContext.PlexTvShowEpisodes.FirstOrDefaultAsync(
                    x => x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId,
                    cancellationToken
                );
                if (entity is not null)
                    return Result.Ok(entity.Id);

                break;
            }
            default:
                return Result.Fail($"Type {mediaType} is not supported for retrieving the plexMediaId by key");
        }

        return Result.Fail(
            $"Couldn't find a plexMediaId with key {plexApiRatingKey}, plexServerId {plexServerId} with type {mediaType}"
        );
    }

    /// <summary>
    /// Bulk inserts the Plex movies and the movie media data into the database.
    /// </summary>
    public static async Task<Result> BulkInsertPlexMoviesAsync(
        this IReaparrDbContext context,
        List<PlexMovie> plexMovies,
        int plexServerId,
        int plexLibraryId,
        CancellationToken ct = default
    )
    {
        if (!plexMovies.Any())
            return Result.Fail("No movies to insert").LogWarning();

        if (plexServerId == 0)
            return ResultExtensions.IsZero(nameof(plexServerId));

        if (plexLibraryId == 0)
            return ResultExtensions.IsZero(nameof(plexLibraryId));

        return await Result.Try(async Task () =>
        {
            plexMovies.SetRelationshipIds(plexServerId, plexLibraryId);

            await context.BulkInsertAsync(plexMovies, BulkConfigPreset.Default, ct);

            // Add movie media data for each movie
            var mediaData = plexMovies
                .SelectMany(x =>
                {
                    x.MediaDataList.SetRelationshipIds(x.PlexServerId, x.PlexLibraryId, x.Id);
                    return x.MediaDataList;
                })
                .ToList();

            await context.BulkInsertAsync(mediaData, BulkConfigPreset.Default, ct);
        });
    }
}
