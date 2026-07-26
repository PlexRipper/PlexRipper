namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<string> GetPlexLibraryNameById(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        CancellationToken cancellationToken = default
    )
    {
        var plexLibraryName = await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(cancellationToken);
        return plexLibraryName ?? "Library Name Not Found";
    }

    public static async Task<int> GetPlexServerIdFromPlexLibraryId(this IReaparrDbContext dbContext, int plexLibraryId)
    {
        return await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(x => x.PlexServerId)
            .FirstOrDefaultAsync(CancellationToken.None);
    }

    public static async Task<HashSet<int>> GetCurrentOwnedLibraryIds(
        this IReaparrDbContext dbContext,
        int remoteLibraryId,
        PlexMediaType mediaType,
        CancellationToken cancellationToken = default)
    {
        var remoteUpdatedAt = await dbContext.PlexLibraries
            .Where(x => x.Id == remoteLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(cancellationToken);
        if (remoteUpdatedAt is null)
            return [];

        var ownedLibraries = await dbContext.PlexLibraries
            .WhereIsOwned()
            .Where(x => x.Type == mediaType)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, cancellationToken);

        var scopeRows = await dbContext.PlexComparisonScopes
            .Where(x => x.RemotePlexLibraryId == remoteLibraryId && x.MediaType == mediaType &&
                        ownedLibraries.Keys.Contains(x.OwnedPlexLibraryId))
            .ToListAsync(cancellationToken);

        return scopeRows
            .Where(x =>
                x.RemoteLibraryUpdatedAt == remoteUpdatedAt
                && ownedLibraries.TryGetValue(x.OwnedPlexLibraryId, out var ownedUpdatedAt)
                && x.OwnedLibraryUpdatedAt == ownedUpdatedAt)
            .Select(x => x.OwnedPlexLibraryId)
            .ToHashSet();
    }

    public static async Task<HashSet<int>> GetCurrentRemoteLibraryIds(
        this IReaparrDbContext dbContext,
        int ownedLibraryId,
        PlexMediaType mediaType,
        CancellationToken cancellationToken = default)
    {
        var ownedUpdatedAt = await dbContext.PlexLibraries
            .Where(x => x.Id == ownedLibraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(cancellationToken);
        if (ownedUpdatedAt is null)
            return [];

        var remoteLibraries = await dbContext.PlexLibraries
            .WhereIsNotOwned()
            .Where(x => x.Type == mediaType)
            .Select(x => new { x.Id, x.UpdatedAt })
            .ToDictionaryAsync(x => x.Id, x => x.UpdatedAt, cancellationToken);

        var scopeRows = await dbContext.PlexComparisonScopes
            .Where(x => x.OwnedPlexLibraryId == ownedLibraryId && x.MediaType == mediaType &&
                        remoteLibraries.Keys.Contains(x.RemotePlexLibraryId))
            .ToListAsync(cancellationToken);

        return scopeRows
            .Where(x =>
                x.OwnedLibraryUpdatedAt == ownedUpdatedAt
                && remoteLibraries.TryGetValue(x.RemotePlexLibraryId, out var remoteUpdatedAt)
                && x.RemoteLibraryUpdatedAt == remoteUpdatedAt)
            .Select(x => x.RemotePlexLibraryId)
            .ToHashSet();
    }

    public static async Task SetLibraryMetaData(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        int actorsCount,
        int genreCount,
        int countryCount
    )
    {
        await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.ActorsCount, actorsCount)
                    .SetProperty(x => x.GenresCount, genreCount)
                    .SetProperty(x => x.CountriesCount, countryCount)
            );
    }

    public static async Task SetMovieMediaMetrics(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        int movieCount,
        long mediaSize
    )
    {
        await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.MovieCount, movieCount).SetProperty(x => x.MediaSize, mediaSize));
    }

    public static async Task SetTvShowMediaMetrics(
        this IReaparrDbContext dbContext,
        int plexLibraryId,
        int tvShowCount,
        int seasonCount,
        int episodeCount,
        long mediaSize
    )
    {
        await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.TvShowCount, tvShowCount)
                    .SetProperty(x => x.SeasonCount, seasonCount)
                    .SetProperty(x => x.EpisodeCount, episodeCount)
                    .SetProperty(x => x.MediaSize, mediaSize)
            );
    }
}
