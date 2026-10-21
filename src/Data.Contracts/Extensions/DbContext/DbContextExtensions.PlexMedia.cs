namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<int>> GetPlexMediaByRatingKeyAsync(
        this IReaparrDbContext dbContext,
        int plexApiRatingKey,
        int plexServerId,
        PlexMediaType mediaType
    )
    {
        Result<int> ToLookupResult(int id) =>
            id == 0
                ? Result.Fail<int>(
                    $"Media with rating key {plexApiRatingKey} was not found on server {plexServerId} for type {mediaType}"
                )
                : Result.Ok(id);

        switch (mediaType)
        {
            case PlexMediaType.Movie:
            {
                var id = await dbContext
                    .PlexMovies.Where(x => x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.TvShow:
            {
                var id = await dbContext
                    .PlexTvShows.Where(x => x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.Season:
            {
                var id = await dbContext
                    .PlexTvShowSeason.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.Episode:
            {
                var id = await dbContext
                    .PlexTvShowEpisodes.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.MusicArtist:
            {
                var id = await dbContext
                    .PlexMusicArtists.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.MusicAlbum:
            {
                var id = await dbContext
                    .PlexMusicAlbums.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.MusicTrack:
            {
                var id = await dbContext
                    .PlexMusicTracks.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.PhotoAlbum:
            {
                var id = await dbContext
                    .PlexPhotoAlbums.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.PhotoImage:
            {
                var id = await dbContext
                    .PlexPhotoMediaData.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            case PlexMediaType.OtherVideos:
            {
                var id = await dbContext
                    .PlexOtherVideos.Where(x =>
                        x.PlexApiRatingKey == plexApiRatingKey && x.PlexServerId == plexServerId
                    )
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken.None);
                return ToLookupResult(id);
            }
            default:
                return Result.Fail(
                    "Type {PlexMediaType} is not supported for retrieving the plexMediaId by key",
                    mediaType
                );
        }
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

        return await context.ExecuteTransactionAsync(
            async (ctx, txCt) =>
            {
                plexMovies.SetRelationshipIds(plexServerId, plexLibraryId);

                foreach (var movie in plexMovies)
                    movie.VideoQuality =
                        movie.MediaDataList.Count == 0
                            ? VideoQuality.Unknown
                            : movie.MediaDataList.Max(x => x.VideoQuality);

                await ctx.BulkInsertAsync(plexMovies, BulkConfigPreset.Default, txCt);

                // Add movie media data for each movie
                var mediaData = plexMovies
                    .SelectMany(x =>
                    {
                        x.MediaDataList.SetRelationshipIds(x.PlexServerId, x.PlexLibraryId, x.Id);
                        return x.MediaDataList;
                    })
                    .ToList();

                await ctx.BulkInsertAsync(mediaData, BulkConfigPreset.Default, txCt);
            },
            ct
        );
    }

    /// <summary>
    /// Deletes a photo library's albums, image and clip assets, album memberships, and descendant folders.
    /// Source-root folders are retained and marked inactive.
    /// </summary>
    /// <param name="dbContext">The database context used to purge the library's media.</param>
    /// <param name="plexLibraryId">The ID of the photo library to purge.</param>
    /// <param name="cancellationToken">The cancellation token for the database operations.</param>
    /// <returns>The number of deleted albums and assets, excluding memberships and folders.</returns>
    /// <remarks>The caller must provide the transaction when atomic deletion is required.</remarks>
    public static async Task<int> DeletePlexPhotoLibraryMediaAsync(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        CancellationToken cancellationToken
    )
    {
        var albumCount = await dbContext.PlexPhotoAlbums.CountAsync(
            x => x.PlexLibraryId == plexLibraryId,
            cancellationToken
        );
        var fileCount = await dbContext.PlexPhotoMediaData.CountAsync(
            x => x.PlexLibraryId == plexLibraryId,
            cancellationToken
        );
        var descendantFolderIds = await dbContext
            .PlexPhotoFolders.Where(x => x.PlexLibraryId == plexLibraryId && x.ParentId != null)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        await dbContext
            .PlexPhotoAlbumMemberships.Where(x => x.PlexPhotoAlbum!.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext
            .PlexPhotoMediaData.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext
            .PlexPhotoAlbums.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);
        if (descendantFolderIds.Count > 0)
        {
            await dbContext
                .PlexPhotoFolders.Where(x => descendantFolderIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await dbContext
            .PlexPhotoFolders.Where(x => x.PlexLibraryId == plexLibraryId && x.ParentId == null)
            .ExecuteUpdateAsync(x => x.SetProperty(root => root.IsActive, false), cancellationToken);

        return albumCount + fileCount;
    }
}
