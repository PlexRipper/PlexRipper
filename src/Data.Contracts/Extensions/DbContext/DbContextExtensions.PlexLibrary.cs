namespace Reaparr.Data.Contracts;

public static partial class DbContextExtensions
{
    public static async Task<string> GetPlexLibraryNameById(
        this IReaparrDbContext dbContext,
        int plexLibraryId
    )
    {
        var plexLibraryName = await dbContext
            .PlexLibraries.IgnoreIsEnabledFilter()
            .Where(x => x.Id == plexLibraryId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(CancellationToken.None);
        return plexLibraryName ?? "Library Name Not Found";
    }

    public static async Task<int> GetPlexServerIdFromPlexLibraryId(
        this IReaparrDbContext dbContext,
        int plexLibraryId
    )
    {
        return await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(x => x.PlexServerId)
            .FirstOrDefaultAsync(CancellationToken.None);
    }

    public static Task<HashSet<int>> GetCurrentOwnedLibraryIds(
        this IReaparrDbContext dbContext,
        int remoteLibraryId,
        PlexMediaType mediaType,
        CancellationToken cancellationToken = default) => GetCurrentLibraryIds(
        dbContext,
        CurrentLibraryRole.Owned,
        remoteLibraryId,
        mediaType,
        cancellationToken);

    public static Task<HashSet<int>> GetCurrentRemoteLibraryIds(
        this IReaparrDbContext dbContext,
        int ownedLibraryId,
        PlexMediaType mediaType,
        CancellationToken cancellationToken = default) => GetCurrentLibraryIds(
        dbContext,
        CurrentLibraryRole.Remote,
        ownedLibraryId,
        mediaType,
        cancellationToken);

    private static async Task<HashSet<int>> GetCurrentLibraryIds(
        IReaparrDbContext dbContext,
        CurrentLibraryRole role,
        int libraryId,
        PlexMediaType mediaType,
        CancellationToken cancellationToken)
    {
        var libraryUpdatedAt = await dbContext.PlexLibraries
            .Where(x => x.Id == libraryId)
            .Select(x => x.UpdatedAt)
            .SingleOrDefaultAsync(cancellationToken);

        if (libraryUpdatedAt is null)
            return [];

        var ids = role == CurrentLibraryRole.Owned
            ? await dbContext.PlexComparisonScopes
                .Where(x => x.RemotePlexLibraryId == libraryId
                            && x.MediaType == mediaType
                            && x.RemoteLibraryUpdatedAt == libraryUpdatedAt)
                .Join(
                    dbContext.PlexLibraries.WhereIsOwned().Where(x => x.Type == mediaType),
                    scope => scope.OwnedPlexLibraryId,
                    library => library.Id,
                    (scope, library) => new { scope, library })
                .Where(x => x.scope.OwnedLibraryUpdatedAt == x.library.UpdatedAt)
                .Select(x => x.library.Id)
                .ToListAsync(cancellationToken)
            : await dbContext.PlexComparisonScopes
                .Where(x => x.OwnedPlexLibraryId == libraryId
                            && x.MediaType == mediaType
                            && x.OwnedLibraryUpdatedAt == libraryUpdatedAt)
                .Join(
                    dbContext.PlexLibraries.WhereIsNotOwned().Where(x => x.Type == mediaType),
                    scope => scope.RemotePlexLibraryId,
                    library => library.Id,
                    (scope, library) => new { scope, library })
                .Where(x => x.scope.RemoteLibraryUpdatedAt == x.library.UpdatedAt)
                .Select(x => x.library.Id)
                .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    private enum CurrentLibraryRole
    {
        Owned,
        Remote,
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
