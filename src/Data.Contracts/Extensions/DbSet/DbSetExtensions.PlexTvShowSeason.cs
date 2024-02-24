using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexTvShowSeason> IncludePlexLibrary(this IQueryable<PlexTvShowSeason> plexTvShowSeason) =>
        plexTvShowSeason.Include(x => x.PlexLibrary);

    public static IQueryable<PlexTvShowSeason> IncludePlexServer(this IQueryable<PlexTvShowSeason> plexTvShowSeason) =>
        plexTvShowSeason.Include(x => x.PlexServer).ThenInclude(x => x.PlexServerConnections);

    public static IQueryable<PlexTvShowSeason> IncludeEpisodes(this IQueryable<PlexTvShowSeason> plexTvShowSeason) => plexTvShowSeason.Include(x => x.Episodes);
}