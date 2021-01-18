using System.Linq;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Data.Common
{
    public static class PlexRipperDbContextExtensions
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

        public static IQueryable<PlexLibrary> IncludeMovies(this IQueryable<PlexLibrary> plexLibrary, bool topLevelOnly = false)
        {
            if (topLevelOnly)
            {
                return plexLibrary
                    .Include(x => x.Movies);
            }

            return plexLibrary
                .Include(x => x.Movies)
                .ThenInclude(x => x.PlexMovieDatas)
                .ThenInclude(x => x.Parts);
        }

        public static IQueryable<PlexMovie> IncludeMovieData(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies
                .Include(x => x.PlexMovieDatas)
                .ThenInclude(x => x.Parts);
        }

        public static IQueryable<PlexMovie> IncludePlexLibrary(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies
                .Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexMovie> IncludeServer(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies
                .Include(x => x.PlexLibrary)
                .ThenInclude(x => x.PlexServer);
        }

        public static IQueryable<PlexLibrary> IncludeServer(this IQueryable<PlexLibrary> plexLibrary)
        {
            return plexLibrary
                .Include(x => x.PlexServer);
        }
    }
}