using System.IO.Abstractions.TestingHelpers;
using Data.Contracts;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

public class UnitTestDataConfig : BaseConfig<UnitTestDataConfig>
{
    public Action<FakeDataConfig>? DatabaseOptions { get; set; } = null;

    public IConfigManager? MockConfigManager { get; set; }

    public int DownloadSpeedLimitInKib { get; set; } = 0;

    public int PlexServerSettingsCount { get; set; } = 5;

    public Action<Mock<HttpMessageHandler>, IPlexRipperDbContext>? HttpClientOptions { get; set; }

    public Action<PlexApiDataConfig>? BaseMockHttpClientOptions { get; set; }

    public Action<MockFileSystem, IPlexRipperDbContext>? FileSystemOptions { get; set; }
}
