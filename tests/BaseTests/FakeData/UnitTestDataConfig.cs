using System;
using JetBrains.Annotations;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;

namespace PlexRipper.BaseTests
{
    public class UnitTestDataConfig : IDisposable
    {
        public UnitTestDataConfig() { }

        public UnitTestDataConfig(int seed)
        {
            Seed = seed;
        }

        public string MemoryDbName { get; set; } = MockDatabase.GetMemoryDatabaseName();

        public int Seed { get; set; }

        /// <summary>
        ///     If none, a random will be picked
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

        public int TvShowSeasonCount { get; set; } = 2;

        public int TvShowEpisodeCount { get; set; } = 5;

        #region DownloadTasks

        public int MovieDownloadTasksCount { get; set; } = 0;

        public int TvShowDownloadTasksCount { get; set; } = 0;

        public int TvShowSeasonDownloadTasksCount { get; set; } = 2;

        public int TvShowEpisodeDownloadTasksCount { get; set; } = 5;

        #endregion

        #endregion

        #endregion

        #region Mocks

        public IFileSystem MockFileSystem { get; set; }

        public IDownloadSubscriptions MockDownloadSubscriptions { get; set; }

        public IConfigManager MockConfigManager { get; set; }

        #endregion

        #region MockServer

        public PlexMockServer MockServer { get; private set; }

        public PlexMockServerConfig MockServerConfig { get; set; }

        public void SetupMockServer([CanBeNull] Action<PlexMockServerConfig> options = null)
        {
            var config = new PlexMockServerConfig();
            options?.Invoke(config);

            MockServer = new PlexMockServer(config);
        }

        #endregion

        #region UserSettings

        public ISettingsModel UserSettings { get; set; }

        public int DownloadSpeedLimit { get; set; } = 0;

        public int PlexServerSettingsCount { get; set; } = 5;

        #endregion

        public static UnitTestDataConfig FromOptions(Action<UnitTestDataConfig> action = null)
        {
            var config = new UnitTestDataConfig();
            action?.Invoke(config);
            return config;
        }

        public void Dispose()
        {
            MockServer?.Dispose();
        }
    }
}