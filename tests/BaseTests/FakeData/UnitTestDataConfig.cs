using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class UnitTestDataConfig
    {
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

        public int TvShowSeasonCountMax { get; init; } = 5;

        public int TvShowEpisodeCountMax { get; init; } = 5;

        public int MovieDownloadTasksCount { get; init; } = 0;

        public int TvShowDownloadTasksCount { get; init; } = 0;

        #endregion

        #endregion

        #region Mocks

        public IFileSystem MockFileSystem { get; init; }

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