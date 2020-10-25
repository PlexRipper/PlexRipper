using System.Linq;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Data.Common
{
    public static class PlexRipperDbContextExtensions
    {
        public static IQueryable<PlexLibrary> IncludeMedia(this IQueryable<PlexLibrary> plexLibrary)
        {
            return plexLibrary
                .Include(x => x.Movies)
                .ThenInclude(x => x.PlexMovieDatas)
                .ThenInclude(x => x.Parts)
                .Include(x => x.TvShows)
                .ThenInclude(x => x.Seasons)
                .ThenInclude(x => x.Episodes);
        }

        public static IQueryable<PlexMovie> IncludeMovieData(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies
                .Include(x => x.PlexMovieDatas)
                .ThenInclude(x => x.Parts);
        }

        public static IQueryable<PlexLibrary> IncludeServer(this IQueryable<PlexLibrary> plexLibrary)
        {
            return plexLibrary
                .Include(x => x.PlexServer);
        }
    }
}