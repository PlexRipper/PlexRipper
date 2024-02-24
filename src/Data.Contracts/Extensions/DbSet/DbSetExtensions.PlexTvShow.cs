using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexTvShow> IncludeEpisodes(this IQueryable<PlexTvShow> plexTvShows)
    {
        return plexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes);
    }

    public static IQueryable<PlexTvShow> IncludePlexServer(this IQueryable<PlexTvShow> plexTvShows)
    {
        return plexTvShows.Include(x => x.PlexServer).ThenInclude(x => x.PlexServerConnections);
    }

    public static IQueryable<PlexTvShow> IncludePlexLibrary(this IQueryable<PlexTvShow> plexTvShows)
    {
        return plexTvShows.Include(x => x.PlexLibrary);
    }
}