using PlexRipper.Application;
using PlexRipper.DownloadManager;

// ReSharper disable RedundantDefaultMemberInitializer

namespace PlexRipper.BaseTests;

public class UnitTestDataConfig : BaseConfig<UnitTestDataConfig>
{
    #region Mocks

    public IFileSystem MockFileSystem { get; set; }

    public IDownloadSubscriptions MockDownloadSubscriptions { get; set; }

    public IConfigManager MockConfigManager { get; set; }

    #endregion

    #region UserSettings

    public ISettingsModel UserSettings { get; set; }

    public int DownloadSpeedLimit { get; set; } = 0;

    public int PlexServerSettingsCount { get; set; } = 5;

    #endregion
}