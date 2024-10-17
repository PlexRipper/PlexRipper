using FileSystem.Contracts;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

public class UnitTestDataConfig : BaseConfig<UnitTestDataConfig>
{
    public Action<FakeDataConfig>? DatabaseOptions { get; set; } = null;

    public IFileSystem? MockFileSystem { get; set; }

    public IConfigManager? MockConfigManager { get; set; }

    public int DownloadSpeedLimitInKib { get; set; } = 0;

    public int PlexServerSettingsCount { get; set; } = 5;

    public Action<Mock<HttpMessageHandler>>? HttpClientOptions { get; set; }
}
