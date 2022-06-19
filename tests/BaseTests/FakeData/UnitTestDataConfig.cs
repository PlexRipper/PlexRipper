using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;

namespace PlexRipper.BaseTests
{
    public class UnitTestDataConfig
    {
        public UnitTestDataConfig() { }

        public UnitTestDataConfig(int seed)
        {
            Seed = seed;
        }

        public string MemoryDbName { get; init; } = MockDatabase.GetMemoryDatabaseName();

        public int Seed { get; init; }

        /// <summary>
        /// If none, a random will be picked
        /// </summary>
        public PlexMediaType LibraryType { get; init; } = PlexMediaType.None;

        #region Include

        public bool IncludeLibraries { get; init; } = true;

        public bool IncludeDownloadTasks { get; init; }

        public bool IncludeMultiPartMovies { get; init; }

        #endregion

        #region Count

        public int PlexServerCount { get; init; } = 1;

        public int PlexLibraryCount { get; init; } = 0;

        public int DownloadTasksCount { get; init; } = 10;

        #region Opt-in

        public int MovieCount { get; init; } = 0;

        public int TvShowCount { get; init; } = 0;

        public int TvShowSeasonCount { get; init; } = 2;

        public int TvShowEpisodeCount { get; init; } = 5;

        #region DownloadTasks

        public int MovieDownloadTasksCount { get; init; } = 0;

        public int TvShowDownloadTasksCount { get; init; } = 0;

        public int TvShowSeasonDownloadTasksCount { get; init; } = 2;

        public int TvShowEpisodeDownloadTasksCount { get; init; } = 5;

        #endregion

        #endregion

        #endregion

        #region Mocks

        public IFileSystem MockFileSystem { get; init; }

        public IDownloadSubscriptions MockDownloadSubscriptions { get; init; }

        public IConfigManager MockConfigManager { get; init; }

        #endregion

        #region MockServer

        public PlexMockServerConfig MockServerConfig { get; init; }

        #endregion

        #region UserSettings

        public ISettingsModel UserSettings { get; init; }

        public int DownloadSpeedLimit { get; init; }

        #endregion
    }
}