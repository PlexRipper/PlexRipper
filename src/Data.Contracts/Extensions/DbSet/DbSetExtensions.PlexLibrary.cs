using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexLibrary> IncludeTvShows(this IQueryable<PlexLibrary> plexLibrary, bool topLevelOnly = false)
    {
        if (topLevelOnly)
        {
            return plexLibrary
                .Include(x => x.TvShows);
        }

        return plexLibrary
            .Include(x => x.TvShows)
            .ThenInclude(x => x.Seasons)
            .ThenInclude(x => x.Episodes);
    }

    public static IQueryable<PlexLibrary> IncludeMovies(this IQueryable<PlexLibrary> plexLibrary) => plexLibrary
        .Include(x => x.Movies);

    public static IQueryable<PlexLibrary> IncludePlexServer(this IQueryable<PlexLibrary> plexLibrary) =>
        plexLibrary.Include(x => x.PlexServer).ThenInclude(x => x.PlexServerConnections);
}