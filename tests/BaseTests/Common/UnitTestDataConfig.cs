using FileSystem.Contracts;
using Settings.Contracts;

// ReSharper disable RedundantDefaultMemberInitializer

namespace PlexRipper.BaseTests;

public class UnitTestDataConfig : BaseConfig<UnitTestDataConfig>
{
    #region Mocks

    public IFileSystem MockFileSystem { get; set; }

    public IConfigManager MockConfigManager { get; set; }

    #endregion

    #region UserSettings

    public int DownloadSpeedLimitInKib { get; set; } = 0;

    public int PlexServerSettingsCount { get; set; } = 5;

    #endregion
}
