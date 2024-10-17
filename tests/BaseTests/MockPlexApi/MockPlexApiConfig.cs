namespace PlexRipper.BaseTests;

public class MockPlexApiConfig : BaseConfig<MockPlexApiConfig>
{
    public List<PlexMockServerConfig> MockServers { get; set; } = [];
}
