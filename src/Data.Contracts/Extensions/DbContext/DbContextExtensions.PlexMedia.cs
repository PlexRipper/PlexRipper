using Application.Contracts;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<Result<int>> GetPlexMediaByMediaKeyAsync(
        this IPlexRipperDbContext dbContext,
        int plexMediaKey,
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
                    x => x.Key == plexMediaKey && x.PlexServerId == plexServerId,
                    cancellationToken
                );
                if (entity is not null)
                    return Result.Ok(entity.Id);

                break;
            }
            case PlexMediaType.TvShow:
            {
                var entity = await dbContext.PlexTvShows.FirstOrDefaultAsync(
                    x => x.Key == plexMediaKey && x.PlexServerId == plexServerId,
                    cancellationToken
                );
                if (entity is not null)
                    return Result.Ok(entity.Id);

                break;
            }
            case PlexMediaType.Season:
            {
                var entity = await dbContext.PlexTvShowSeason.FirstOrDefaultAsync(
                    x => x.Key == plexMediaKey && x.PlexServerId == plexServerId,
                    cancellationToken
                );
                if (entity is not null)
                    return Result.Ok(entity.Id);

                break;
            }
            case PlexMediaType.Episode:
            {
                var entity = await dbContext.PlexTvShowEpisodes.FirstOrDefaultAsync(
                    x => x.Key == plexMediaKey && x.PlexServerId == plexServerId,
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
            $"Couldn't find a plexMediaId with key {plexMediaKey}, plexServerId {plexServerId} with type {mediaType}"
        );
    }

    public static async Task<Result<List<PlexMediaSlimDTO>>> GetMediaByType(
        this IPlexRipperDbContext dbContext,
        MediaQueryFilter filter,
        CancellationToken ct = default
    )
    {
        List<PlexMediaSlimDTO> plexMediaSlimDtos;
        var plexLibraryId = filter.PlexLibraryId;

        var serverList = await dbContext
            .PlexServers.Where(x => x.IsEnabled)
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
            return Result.Ok(new List<PlexMediaSlimDTO>());

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

                plexMediaSlimDtos = await query
                    .Include(x => x.MediaDataList)
                    .ApplyWhere(plexLibraryId > 0, x => x.PlexLibraryId == plexLibraryId)
                    .ApplyWhere(plexLibraryId == 0, x => allowedPlexLibraryIds.Contains(x.PlexLibraryId))
                    .ApplyWhere(filter.CountryId > 0, x => x.Countries.Any(y => y.Id == filter.CountryId))
                    .ApplyWhere(filter.GenreId > 0, x => x.Genres.Any(y => y.Id == filter.GenreId))
                    .ApplyWhere(filter.ActorId > 0, x => x.Actors.Any(y => y.Id == filter.ActorId))
                    .ApplyOrderBy(plexLibraryId > 0, x => x.SortIndex)
                    .ApplySkip(filter.Skip)
                    .ApplyTake(filter.Take)
                    .ProjectToMediaSlimDTO()
                    .ToListAsync(ct);

                break;
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

                plexMediaSlimDtos = await query
                    .Include(x => x.Qualities)
                    .ApplyWhere(plexLibraryId > 0, x => x.PlexLibraryId == plexLibraryId)
                    .ApplyWhere(plexLibraryId == 0, x => allowedPlexLibraryIds.Contains(x.PlexLibraryId))
                    .ApplyWhere(filter.CountryId > 0, x => x.Countries.Any(y => y.Id == filter.CountryId))
                    .ApplyWhere(filter.GenreId > 0, x => x.Genres.Any(y => y.Id == filter.GenreId))
                    .ApplyWhere(filter.ActorId > 0, x => x.Actors.Any(y => y.Id == filter.ActorId))
                    .ApplyOrderBy(plexLibraryId > 0, x => x.SortIndex)
                    .ApplySkip(filter.Skip)
                    .ApplyTake(filter.Take)
                    .ProjectToMediaSlimDTO()
                    .ToListAsync(ct);
                break;
            }
            default:
                return Result.Fail(
                    $"Type {filter.MediaType} is not supported for retrieving the PlexMedia data by library id"
                );
        }

        if (plexLibraryId == 0)
            plexMediaSlimDtos = plexMediaSlimDtos.OrderByNatural(x => x.SearchTitle).ToList();

        // Add token to retrieve thumbnail in front-end
        Dictionary<int, string> tokensCache = new();

        for (var i = 0; i < plexMediaSlimDtos.Count; i++)
        {
            var slimDTO = plexMediaSlimDtos[i];

            slimDTO.SortIndex = i + 1;

            if (tokensCache.TryGetValue(slimDTO.PlexServerId, out var token))
            {
                slimDTO.PlexToken = token;
                continue;
            }

            var result = await dbContext.GetPlexServerTokenAsync(slimDTO.PlexServerId, ct);
            if (result.IsSuccess)
            {
                tokensCache.Add(slimDTO.PlexServerId, result.Value);
                slimDTO.PlexToken = result.Value;
            }
        }

        // If the plexLibraryId is set, we don't need to sort the list again
        return Result.Ok(plexMediaSlimDtos);
    }

    /// <summary>
    /// Bulk inserts the Plex movies and the movie media data into the database.
    /// </summary>
    public static async Task<Result> BulkInsertPlexMoviesAsync(
        this IPlexRipperDbContext context,
        List<PlexMovie> plexMovies,
        int plexServerId,
        int plexLibraryId,
        CancellationToken ct = default
    )
    {
        try
        {
            if (!plexMovies.Any())
                return Result.Fail("No movies to insert").LogWarning();

            if (plexServerId == 0)
                return ResultExtensions.IsZero(nameof(plexServerId));

            if (plexLibraryId == 0)
                return ResultExtensions.IsZero(nameof(plexLibraryId));

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

            // Add movie media data parts for each media data
            var parts = mediaData
                .SelectMany(x =>
                {
                    x.Parts.SetRelationshipIds(x.PlexServerId, x.PlexLibraryId, x.PlexMovieId, x.Id);
                    return x.Parts;
                })
                .ToList();
            await context.BulkInsertAsync(parts, BulkConfigPreset.Default, ct);

            // Add movie media data streams for each part
            var streams = parts
                .SelectMany(x =>
                {
                    x.Streams.SetRelationshipIds(
                        x.PlexServerId,
                        x.PlexLibraryId,
                        x.PlexMovieId,
                        x.PlexMovieMediaDataId,
                        x.Id
                    );
                    return x.Streams;
                })
                .ToList();
            await context.BulkInsertAsync(streams, BulkConfigPreset.Default, ct);

            return Result.Ok();
        }
        catch (Exception e)
        {
            _log.Error(
                "Error while bulk inserting plex movies with serverId: {PlexServerId} and libraryId: {PlexLibraryId}",
                plexServerId,
                plexLibraryId
            );
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
