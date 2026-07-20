namespace Reaparr.Data.Contracts;

public static partial class DbSetExtensions
{
    /// <summary>
    /// Filters <see cref="PlexLibrary"/> to only those owned by any linked Plex account.
    /// </summary>
    /// <remarks>
    /// A library is owned when:
    /// <list type="number">
    ///   <item><c>PlexServer.OwnedOverride == true</c>, or</item>
    ///   <item><c>PlexServer.OwnedOverride</c> is null AND
    ///     (<c>PlexAccountLibraries.Any(y => y.IsLibraryOwned)</c>
    ///     OR <c>PlexServer.PlexAccountServers.Any(y => y.IsServerOwned)</c>)</item>
    /// </list>
    /// </remarks>
    public static IQueryable<PlexLibrary> WhereIsOwned(this IQueryable<PlexLibrary> query) =>
        query.Where(x =>
            x.PlexServer!.OwnedOverride == true
            || (x.PlexServer!.OwnedOverride == null
                && (x.PlexAccountLibraries.Any(y => y.IsLibraryOwned)
                    || x.PlexServer.PlexAccountServers.Any(y => y.IsServerOwned)))
        );

    /// <summary>
    /// Filters <see cref="PlexLibrary"/> to only those NOT owned by any linked Plex account.
    /// </summary>
    /// <remarks>
    /// A library is not owned when:
    /// <list type="number">
    ///   <item><c>PlexServer.OwnedOverride == false</c>, or</item>
    ///   <item><c>PlexServer.OwnedOverride</c> is null AND
    ///     (no <c>PlexAccountLibrary.IsLibraryOwned</c>
    ///     AND no <c>PlexAccountServer.IsServerOwned</c>)</item>
    /// </list>
    /// </remarks>
    public static IQueryable<PlexLibrary> WhereIsNotOwned(this IQueryable<PlexLibrary> query) =>
        query.Where(x =>
            x.PlexServer!.OwnedOverride == false
            || (x.PlexServer!.OwnedOverride == null
                && !x.PlexAccountLibraries.Any(y => y.IsLibraryOwned)
                && !x.PlexServer.PlexAccountServers.Any(y => y.IsServerOwned))
        );
}
