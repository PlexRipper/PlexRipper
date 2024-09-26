namespace PlexRipper.BaseTests;

public class MockPlexApiConfig : BaseConfig<MockPlexApiConfig>
{
    public PlexApiDataConfig FakeDataConfig { get; set; }

    public bool SignInResponseIsValid { get; set; } = true;

    public int AccessiblePlexServers { get; set; } = 3;

    public bool UnauthorizedAccessiblePlexServers { get; set; } = false;

    public List<PlexMockServerConfig> MockServers { get; set; } = [];
}
