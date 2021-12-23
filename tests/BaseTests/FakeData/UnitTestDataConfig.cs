using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class UnitTestDataConfig
    {
        public string MemoryDbName { get; init; }

        public int Seed { get; init; }

        public string ServerUrl { get; init; } = "https://test-server.com";

        /// <summary>
        /// If none, a random will be picked
        /// </summary>
        public PlexMediaType LibraryType { get; set; } = PlexMediaType.None;

        #region Include

        public bool IncludeLibraries { get; set; } = true;

        public bool IncludeDownloadTasks { get; set; }

        public bool IncludeMultiPartMovies { get; set; }

        #endregion

        #region Count

        public int PlexServerCount { get; set; } = 1;

        public int PlexLibraryCount { get; set; } = 0;

        public int DownloadTasksCount { get; set; } = 10;

        #region Opt-in

        public int MovieCount { get; set; } = 0;

        public int TvShowCount { get; set; } = 0;

        public int TvShowSeasonCountMax { get; set; } = 5;

        public int TvShowEpisodeCountMax { get; set; } = 5;

        public int MovieDownloadTasksCount { get; set; } = 0;

        public int TvShowDownloadTasksCount { get; set; } = 0;

        #endregion

        #endregion
    }
}