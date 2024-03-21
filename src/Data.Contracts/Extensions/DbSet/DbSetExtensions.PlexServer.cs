using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexServer> IncludeLibraries(this IQueryable<PlexServer> plexServer) => plexServer.Include(x => x.PlexLibraries).AsQueryable();

    public static IQueryable<PlexServer> IncludeLibrariesWithMedia(this IQueryable<PlexServer> plexServer) => plexServer
        .Include(x => x.PlexLibraries)
        .ThenInclude(x => x.Movies)
        .Include(x => x.PlexLibraries)
        .ThenInclude(x => x.TvShows)
        .ThenInclude(x => x.Seasons)
        .ThenInclude(x => x.Episodes);

    public static IQueryable<PlexServer> IncludeConnections(this IQueryable<PlexServer> plexServer) => plexServer
        .Include(x => x.PlexServerConnections)
        .AsQueryable();

    public static IQueryable<PlexServer> IncludeConnectionsWithStatus(this IQueryable<PlexServer> plexServer) => plexServer
        .Include(x => x.PlexServerConnections)
        .ThenInclude(x => x.PlexServerStatus.OrderByDescending(y => y.LastChecked).Take(1))
        .AsQueryable();
}