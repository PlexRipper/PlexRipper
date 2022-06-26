using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common.Constants;
using PlexRipper.Domain;

namespace PlexRipper.Data.Common
{
    public static partial class PlexRipperDbContextExtensions
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

        public static IQueryable<PlexLibrary> IncludePlexServer(this IQueryable<PlexLibrary> plexLibrary)
        {
            return plexLibrary.Include(x => x.PlexServer);
        }

        #endregion

        #region PlexMovie

        public static IQueryable<PlexMovie> IncludePlexLibrary(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexMovie> IncludePlexServer(this IQueryable<PlexMovie> plexMovies)
        {
            return plexMovies.Include(x => x.PlexServer);
        }

        #endregion

        #region PlexTvShow

        public static IQueryable<PlexTvShow> IncludeAll(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows
                .AsTracking()
                .Include(IncludePath.PlexTvShow_PlexServer)
                .Include(IncludePath.PlexTvShow_PlexLibrary)
                .Include(IncludePath.PlexTvShow_Seasons)
                .Include(IncludePath.PlexTvShow_Seasons_PlexServer)
                .Include(IncludePath.PlexTvShow_Seasons_PlexLibrary)
                .Include(IncludePath.PlexTvShow_Seasons_Episodes)
                .Include(IncludePath.PlexTvShow_Seasons_Episodes_TvShow)
                .Include(IncludePath.PlexTvShow_Seasons_Episodes_PlexServer)
                .Include(IncludePath.PlexTvShow_Seasons_Episodes_PlexLibrary);
        }

        public static IQueryable<PlexTvShow> IncludePlexLibrary(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexTvShow> IncludePlexServer(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(x => x.PlexServer);
        }

        public static IQueryable<PlexTvShow> IncludeSeasons(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(IncludePath.PlexTvShow_Seasons);
        }

        public static IQueryable<PlexTvShow> IncludeEpisodes(this IQueryable<PlexTvShow> plexTvShows)
        {
            return plexTvShows.Include(IncludePath.PlexTvShow_Seasons_Episodes);
        }

        #endregion

        #region PlexTvShowSeason

        public static IQueryable<PlexTvShowSeason> IncludePlexLibrary(this IQueryable<PlexTvShowSeason> plexTvShowSeason)
        {
            return plexTvShowSeason.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexTvShowSeason> IncludePlexServer(this IQueryable<PlexTvShowSeason> plexTvShowSeason)
        {
            return plexTvShowSeason.Include(x => x.PlexServer);
        }

        public static IQueryable<PlexTvShowSeason> IncludeEpisodes(this IQueryable<PlexTvShowSeason> plexTvShowSeason)
        {
            return plexTvShowSeason.Include(x => x.Episodes);
        }

        #endregion

        #region PlexTvShowEpisode

        public static IQueryable<PlexTvShowEpisode> IncludeAll(this IQueryable<PlexTvShowEpisode> plexTvShowEpisodes)
        {
            return plexTvShowEpisodes
                .Include(IncludePath.PlexTvShowEpisode_PlexServer)
                .Include(IncludePath.PlexTvShowEpisode_PlexLibrary)
                .Include(IncludePath.PlexTvShowEpisode_TvShowSeason_PlexServer)
                .Include(IncludePath.PlexTvShowEpisode_TvShowSeason_PlexLibrary)
                .Include(IncludePath.PlexTvShowEpisode_TvShow_PlexServer)
                .Include(IncludePath.PlexTvShowEpisode_TvShow_PlexLibrary);
        }

        public static IQueryable<PlexTvShowEpisode> IncludePlexLibrary(this IQueryable<PlexTvShowEpisode> plexTvShowEpisode)
        {
            return plexTvShowEpisode.Include(x => x.PlexLibrary);
        }

        public static IQueryable<PlexTvShowEpisode> IncludePlexServer(this IQueryable<PlexTvShowEpisode> plexTvShowEpisode)
        {
            return plexTvShowEpisode.Include(x => x.PlexServer);
        }

        public static IQueryable<PlexTvShowEpisode> IncludeTvShow(this IQueryable<PlexTvShowEpisode> plexTvShowEpisodes)
        {
            return plexTvShowEpisodes.Include(x => x.TvShow);
        }

        public static IQueryable<PlexTvShowEpisode> IncludeSeason(this IQueryable<PlexTvShowEpisode> plexTvShowEpisodes)
        {
            return plexTvShowEpisodes.Include(x => x.TvShowSeason);
        }

        #endregion
    }
}