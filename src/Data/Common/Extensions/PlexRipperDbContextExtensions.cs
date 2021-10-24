using System.Linq;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Data.Common
{
    public static class PlexRipperDbContextExtensions
    {
        #region PlexServer

        public static IQueryable<PlexServer> IncludeLibrariesWithMedia(this IQueryable<PlexServer> plexServer)
        {
            return plexServer
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.Movies)
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.TvShows)
                .ThenInclude(x => x.Seasons)
                .ThenInclude(x => x.Episodes);
        }

        public static IQueryable<PlexServer> IncludeLibrariesWithDownloadTasks(this IQueryable<PlexServer> plexServer)
        {
            return plexServer
                .Include(x => x.PlexLibraries)
                .ThenInclude(x => x.DownloadTasks)
                .ThenInclude(x => x.DownloadWorkerTasks);
        }

        #endregion

        #region PlexLibrary

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

        public static IQueryable<PlexLibrary> IncludeMovies(this IQueryable<PlexLibrary> plexLibrary)
        {
            return plexLibrary
                .Include(x => x.Movies);
        }

        public static IQueryable<PlexLibrary> IncludeServer(this IQueryable<PlexLibrary> plexLibrary)
        {
            return plexLibrary.Include(x => x.PlexServer);
        }

        #endregion

        #region PlexMovie

        public static IQueryable<PlexMovie> IncludePlexLibrary(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexMovie> IncludeServer(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies.Include(x => x.PlexServer);
        }

        #endregion

        #region PlexTvShow

        public static IQueryable<PlexTvShow> IncludePlexLibrary(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexTvShow> IncludeServer(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(x => x.PlexServer);
        }

        public static IQueryable<PlexTvShow> IncludeSeasons(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(x => x.Seasons);
        }

        public static IQueryable<PlexTvShow> IncludeEpisodes(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(x => x.Seasons).ThenInclude(x => x.Episodes);
        }

        #endregion

        #region PlexTvShowSeason

        public static IQueryable<PlexTvShowSeason> IncludePlexLibrary(this IQueryable<PlexTvShowSeason> plexTvShowSeason)
        {
            return plexTvShowSeason.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexTvShowSeason> IncludeServer(this IQueryable<PlexTvShowSeason> plexTvShowSeason)
        {
            return plexTvShowSeason.Include(x => x.PlexServer);
        }

        public static IQueryable<PlexTvShowSeason> IncludeEpisodes(this IQueryable<PlexTvShowSeason> plexTvShowSeason)
        {
            return plexTvShowSeason.Include(x => x.Episodes);
        }

        #endregion

        #region PlexTvShowEpisode

        public static IQueryable<PlexTvShowEpisode> IncludePlexLibrary(this IQueryable<PlexTvShowEpisode> plexTvShowEpisode)
        {
            return plexTvShowEpisode.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexTvShowEpisode> IncludeServer(this IQueryable<PlexTvShowEpisode> plexTvShowEpisode)
        {
            return plexTvShowEpisode.Include(x => x.PlexServer);
        }

        #endregion

        #region PlexDownloadTasks

        public static IQueryable<PlexServer> IncludeDownloadTasks(this IQueryable<PlexServer> plexServer, bool includeServer = false,
            bool includeLibrary = false)
        {
            return plexServer
                .Include(x => x.PlexLibraries)
                .IncludeDownloadTasks("PlexLibraries.DownloadTasks.", includeServer, includeLibrary)
                .AsQueryable();
        }

        public static IQueryable<PlexLibrary> IncludeDownloadTasks(
            this IQueryable<PlexLibrary> plexLibrary,
            bool includeServer = false,
            bool includeLibrary = false)
        {
            return plexLibrary.IncludeDownloadTasks("DownloadTasks.", includeServer, includeLibrary);
        }

        public static IQueryable<DownloadTask> IncludeDownloadTasks(
            this IQueryable<DownloadTask> downloadTasks,
            bool includeServer = false,
            bool includeLibrary = false)
        {
            return downloadTasks.IncludeDownloadTasks("", includeServer, includeLibrary);
        }

        private static IQueryable<T> IncludeDownloadTasks<T>(
            this IQueryable<T> query,
            string prefix,
            bool includeServer = false,
            bool includeLibrary = false) where T : class
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                query = query.Include(prefix.TrimEnd('.'));
            }

            // The Include path 'Children->Parent' results in a cycle.
            // Cycles are not allowed in no-tracking queries; either use a tracking query or remove the cycle
            query = query.AsTracking();

            // Include downloadTask children up to 5 levels deep
            for (int i = 1; i <= 5; i++)
            {
                var childPath = prefix + string.Concat(Enumerable.Repeat("Children.", i));

                query = query
                    .Include($"{childPath}".TrimEnd('.'))
                    .Include($"{childPath}DownloadFolder")
                    .Include($"{childPath}DestinationFolder")
                    .Include($"{childPath}DownloadWorkerTasks")
                    .Include($"{childPath}Parent");

                if (includeServer)
                {
                    query = query.Include($"{childPath}PlexServer");
                }

                if (includeLibrary)
                {
                    query = query.Include($"{childPath}PlexLibrary");
                }
            }

            return query;
        }

        #endregion
    }
}