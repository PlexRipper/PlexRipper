using System.IO.Abstractions.TestingHelpers;
using Autofac;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.BaseTests;

public class UnitTestDataConfig : BaseConfig<UnitTestDataConfig>
{
    public Action<FakeDataConfig>? DatabaseOptions { get; set; } = null;

    public IConfigManager? MockConfigManager { get; set; }

    public int DownloadSpeedLimitInKib { get; set; } = 0;

    public int PlexServerSettingsCount { get; set; } = 5;

    public Action<Mock<HttpMessageHandler>, IReaparrDbContext>? HttpClientOptions { get; set; }

    /// <summary>
    /// Set the mock option to create a mock PlexApi Server with the given configuration.
    /// </summary>
    public Action<PlexApiDataConfig>? BaseMockHttpClientOptions { get; set; }

    public Action<MockFileSystem, IReaparrDbContext>? FileSystemOptions { get; set; }

    /// <summary>
    /// Optional per-test Autofac overrides. Invoked after TestModule registration so last registration wins.
    /// </summary>
    public Action<ContainerBuilder>? OverrideServices { get; set; }
}
