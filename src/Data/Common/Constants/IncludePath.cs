using PlexRipper.Domain;

// ReSharper disable InconsistentNaming

namespace PlexRipper.Data.Common.Constants
{
    public static class IncludePath
    {
        #region PlexTvShow

        public static string PlexTvShow_PlexServer =>
            $"{nameof(PlexTvShow.PlexServer)}";

        public static string PlexTvShow_PlexLibrary =>
            $"{nameof(PlexTvShow.PlexLibrary)}";

        public static string PlexTvShow_Seasons =>
            $"{nameof(PlexTvShow.Seasons)}";

        public static string PlexTvShow_Seasons_PlexServer =>
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShow.PlexServer)}";

        public static string PlexTvShow_Seasons_PlexLibrary =>
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShow.PlexLibrary)}";

        public static string PlexTvShow_Seasons_Episodes =>
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShowSeason.Episodes)}";

        public static string PlexTvShow_Seasons_Episodes_TvShow =>
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShowSeason.Episodes)}.{nameof(PlexTvShowEpisode.TvShow)}";

        public static string PlexTvShow_Seasons_Episodes_Season =>
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShowSeason.Episodes)}.{nameof(PlexTvShowEpisode.TvShowSeason)}";

        public static string PlexTvShow_Seasons_Episodes_PlexServer =>
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShowSeason.Episodes)}.{nameof(PlexTvShow.PlexServer)}";

        public static string PlexTvShow_Seasons_Episodes_PlexLibrary =>
            $"{nameof(PlexTvShow.Seasons)}.{nameof(PlexTvShowSeason.Episodes)}.{nameof(PlexTvShow.PlexLibrary)}";

        #endregion

        #region PlexTvShowEpisode

        public static string PlexTvShowEpisode_PlexServer =>
            $"{nameof(PlexTvShowEpisode.PlexServer)}";

        public static string PlexTvShowEpisode_PlexLibrary =>
            $"{nameof(PlexTvShowEpisode.PlexLibrary)}";

        public static string PlexTvShowEpisode_TvShowSeason =>
            $"{nameof(PlexTvShowEpisode.TvShowSeason)}";

        public static string PlexTvShowEpisode_TvShowSeason_PlexServer =>
            $"{nameof(PlexTvShowEpisode.TvShowSeason)}.{nameof(PlexTvShowSeason.PlexServer)}";

        public static string PlexTvShowEpisode_TvShowSeason_PlexLibrary =>
            $"{nameof(PlexTvShowEpisode.TvShowSeason)}.{nameof(PlexTvShowSeason.PlexLibrary)}";

        public static string PlexTvShowEpisode_TvShow_PlexServer =>
            $"{nameof(PlexTvShowEpisode.TvShow)}.{nameof(PlexTvShow.PlexServer)}";

        public static string PlexTvShowEpisode_TvShow_PlexLibrary =>
            $"{nameof(PlexTvShowEpisode.TvShow)}.{nameof(PlexTvShow.PlexLibrary)}";

        #endregion
    }
}