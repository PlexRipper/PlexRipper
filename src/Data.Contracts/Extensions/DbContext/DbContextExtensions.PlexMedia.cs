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

    public static async Task<Result<PagedMediaQueryResult>> GetMediaByType(
        this IReaparrDbContext dbContext,
        MediaQueryFilter filter,
        CancellationToken ct = default
    )
    {
        var plexLibraryId = filter.PlexLibraryId;

        var serverList = await dbContext.PlexServers
            .Select(server => new { server.Id, PlexLibraryIds = server.PlexLibraries.Select(x => x.Id).ToList() })
            .ToListAsync(ct);

        var allowedPlexLibraryIds = serverList.SelectMany(x => x.PlexLibraryIds).ToList();
        if (filter.FilterOwnedMedia)
        {
            var ownedPlexLibraries = await dbContext
                .PlexAccountLibraries.Where(x => x.IsLibraryOwned)
                .Select(x => x.PlexLibraryId)
                .ToListAsync(ct);

            allowedPlexLibraryIds.RemoveAll(x => ownedPlexLibraries.Contains(x));
        }

        if (filter.FilterOfflineMedia)
        {
            foreach (var server in serverList)
            {
                var isServerOnline = await dbContext.IsServerOnline(server.Id, ct);
                if (!isServerOnline)
                {
                    allowedPlexLibraryIds.RemoveAll(x => server.PlexLibraryIds.Contains(x));
                }
            }
        }

        if (plexLibraryId == 0 && !allowedPlexLibraryIds.Any())
            return Result.Ok(new PagedMediaQueryResult { Items = [], TotalCount = 0 });

        var sortField = filter.SortField.Trim();
        var sortDirection = filter.SortDirection.Trim().ToLowerInvariant();
        var search = filter.Search.Trim().ToLowerInvariant();

        switch (filter.MediaType)
        {
            case PlexMediaType.Movie:
            {
                var query = dbContext.PlexMovies.IncludeMediaData().AsNoTracking();

                if (filter.CountryId > 0)
                    query = query.Include(x => x.Countries);

                if (filter.GenreId > 0)
                    query = query.Include(x => x.Genres);

                if (filter.ActorId > 0)
                    query = query.Include(x => x.Actors);

                query = query
                    .Include(x => x.MediaDataList)
                    .ApplyWhere(plexLibraryId > 0, x => x.PlexLibraryId == plexLibraryId)
                    .ApplyWhere(plexLibraryId == 0, x => allowedPlexLibraryIds.Contains(x.PlexLibraryId))
                    .ApplyWhere(filter.CountryId > 0, x => x.Countries.Any(y => y.Id == filter.CountryId))
                    .ApplyWhere(filter.GenreId > 0, x => x.Genres.Any(y => y.Id == filter.GenreId))
                    .ApplyWhere(filter.ActorId > 0, x => x.Actors.Any(y => y.Id == filter.ActorId))
                    .ApplyWhere(filter.Quality != VideoQuality.None, x => x.MediaDataList.Any(y => y.Quality == filter.Quality))
                    .ApplyWhere(!string.IsNullOrWhiteSpace(search), x => x.SearchTitle.ToLower().Contains(search));

                query = ApplyMovieSort(query, sortField, sortDirection);

                var totalCount = await query.CountAsync(ct);
                var take = filter.Take <= 0 ? totalCount : filter.Take;
                var items = await query.ApplySkip(filter.Skip).ApplyTake(take).Select(x => x.ToSlimDTO()).ToListAsync(ct);

                return Result.Ok(new PagedMediaQueryResult { Items = items, TotalCount = totalCount });
            }
            case PlexMediaType.TvShow:
            {
                var query = dbContext.PlexTvShows.AsNoTracking();

                if (filter.CountryId > 0)
                    query = query.Include(x => x.Countries);

                if (filter.GenreId > 0)
                    query = query.Include(x => x.Genres);

                if (filter.ActorId > 0)
                    query = query.Include(x => x.Actors);

                query = query
                    .Include(x => x.Qualities)
                    .ApplyWhere(plexLibraryId > 0, x => x.PlexLibraryId == plexLibraryId)
                    .ApplyWhere(plexLibraryId == 0, x => allowedPlexLibraryIds.Contains(x.PlexLibraryId))
                    .ApplyWhere(filter.CountryId > 0, x => x.Countries.Any(y => y.Id == filter.CountryId))
                    .ApplyWhere(filter.GenreId > 0, x => x.Genres.Any(y => y.Id == filter.GenreId))
                    .ApplyWhere(filter.ActorId > 0, x => x.Actors.Any(y => y.Id == filter.ActorId))
                    .ApplyWhere(filter.Quality != VideoQuality.None, x => x.Qualities.Any(y => y.Quality == filter.Quality))
                    .ApplyWhere(!string.IsNullOrWhiteSpace(search), x => x.SearchTitle.ToLower().Contains(search));

                query = ApplyTvShowSort(query, sortField, sortDirection);

                var totalCount = await query.CountAsync(ct);
                var take = filter.Take <= 0 ? totalCount : filter.Take;
                var items = await query.ApplySkip(filter.Skip).ApplyTake(take).Select(x => x.ToSlimDTOMapper()).ToListAsync(ct);

                return Result.Ok(new PagedMediaQueryResult { Items = items, TotalCount = totalCount });
            }
            default:
                return Result.Fail($"Type {filter.MediaType} is not supported for retrieving the PlexMedia data by library id");
        }
    }

    private static IQueryable<PlexMovie> ApplyMovieSort(IQueryable<PlexMovie> query, string sortField, string sortDirection)
    {
        var asc = sortDirection != "desc";
        return sortField switch
        {
            "year" => asc ? query.OrderBy(x => x.Year).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.Year).ThenByDescending(x => x.SearchTitle),
            "addedAt" => asc ? query.OrderBy(x => x.AddedAt).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.AddedAt).ThenByDescending(x => x.SearchTitle),
            "updatedAt" => asc ? query.OrderBy(x => x.UpdatedAt).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.SearchTitle),
            "duration" => asc ? query.OrderBy(x => x.Duration).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.Duration).ThenByDescending(x => x.SearchTitle),
            "mediaSize" => asc ? query.OrderBy(x => x.MediaSize).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.MediaSize).ThenByDescending(x => x.SearchTitle),
            "quality" => asc
                ? query.OrderBy(x => x.MediaDataList.Max(y => (int?)y.Quality) ?? 0).ThenBy(x => x.SearchTitle)
                : query.OrderByDescending(x => x.MediaDataList.Max(y => (int?)y.Quality) ?? 0).ThenByDescending(x => x.SearchTitle),
            _ => asc ? query.OrderBy(x => x.SortIndex).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.SortIndex).ThenByDescending(x => x.SearchTitle),
        };
    }

    private static IQueryable<PlexTvShow> ApplyTvShowSort(IQueryable<PlexTvShow> query, string sortField, string sortDirection)
    {
        var asc = sortDirection != "desc";
        return sortField switch
        {
            "year" => asc ? query.OrderBy(x => x.Year).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.Year).ThenByDescending(x => x.SearchTitle),
            "addedAt" => asc ? query.OrderBy(x => x.AddedAt).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.AddedAt).ThenByDescending(x => x.SearchTitle),
            "updatedAt" => asc ? query.OrderBy(x => x.UpdatedAt).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.SearchTitle),
            "duration" => asc ? query.OrderBy(x => x.Duration).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.Duration).ThenByDescending(x => x.SearchTitle),
            "mediaSize" => asc ? query.OrderBy(x => x.MediaSize).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.MediaSize).ThenByDescending(x => x.SearchTitle),
            "quality" => asc
                ? query.OrderBy(x => x.Qualities.Max(y => (int?)y.Quality) ?? 0).ThenBy(x => x.SearchTitle)
                : query.OrderByDescending(x => x.Qualities.Max(y => (int?)y.Quality) ?? 0).ThenByDescending(x => x.SearchTitle),
            _ => asc ? query.OrderBy(x => x.SortIndex).ThenBy(x => x.SearchTitle) : query.OrderByDescending(x => x.SortIndex).ThenByDescending(x => x.SearchTitle),
        };
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
