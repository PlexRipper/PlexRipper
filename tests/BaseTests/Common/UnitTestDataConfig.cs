using PlexRipper.Application;
using PlexRipper.DownloadManager;

// ReSharper disable RedundantDefaultMemberInitializer

namespace PlexRipper.BaseTests;

public class UnitTestDataConfig : IDisposable
{
    public UnitTestDataConfig() { }

    public string MemoryDbName { get; set; }

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

    public MockPlexApi MockPlexApi { get; private set; }

    public void SetupMockServer([CanBeNull] Action<PlexMockServerConfig> options = null)
    {
        MockServer = new PlexMockServer(options);
    }

    public void SetupMockPlexApi([CanBeNull] Action<MockPlexApiConfig> options = null)
    {
        MockPlexApi = new MockPlexApi(options);
    }

    #endregion

    #region UserSettings

    public ISettingsModel UserSettings { get; set; }

    public int DownloadSpeedLimit { get; set; } = 0;

    public int PlexServerSettingsCount { get; set; } = 5;

    #endregion

    public static UnitTestDataConfig FromOptions(Action<UnitTestDataConfig> action = null, UnitTestDataConfig defaultValue = null)
    {
        var config = defaultValue ?? new UnitTestDataConfig();
        action?.Invoke(config);
        return config;
    }

    public void Dispose()
    {
        MockServer?.Dispose();
    }

    public int GetSeed()
    {
        return ++Seed;
    }
}