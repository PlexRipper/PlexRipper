using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexMovie> IncludePlexLibrary(this IQueryable<PlexMovie> plexMovies) => plexMovies.Include(x => x.PlexLibrary);

    public static IQueryable<PlexMovie> IncludePlexServer(this IQueryable<PlexMovie> plexMovies) => plexMovies.Include(x => x.PlexServer).ThenInclude(x => x.PlexServerConnections);
}