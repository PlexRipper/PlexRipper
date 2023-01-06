using PlexRipper.Application;
using PlexRipper.DownloadManager;

// ReSharper disable RedundantDefaultMemberInitializer

namespace PlexRipper.BaseTests;

public class UnitTestDataConfig : BaseConfig<UnitTestDataConfig>, IDisposable
{
    #region Mocks

    public IFileSystem MockFileSystem { get; set; }

    public IDownloadSubscriptions MockDownloadSubscriptions { get; set; }

    public IConfigManager MockConfigManager { get; set; }

    #endregion

    public Action<FakeDataConfig> MockDatabase { get; set; } = config => { };

    #region MockServer

    public PlexMockServer MockServer { get; private set; }

    public void SetupMockServer(Action<PlexMockServerConfig> options = null)
    {
        MockServer = new PlexMockServer(options);
    }

    #endregion

    #region UserSettings

    public ISettingsModel UserSettings { get; set; }

    public int DownloadSpeedLimit { get; set; } = 0;

    public int PlexServerSettingsCount { get; set; } = 5;

    #endregion

    public void Dispose()
    {
        MockServer?.Dispose();
    }
}