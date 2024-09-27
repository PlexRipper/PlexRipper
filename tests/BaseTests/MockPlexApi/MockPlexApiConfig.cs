namespace PlexRipper.BaseTests;

public class MockPlexApiConfig : BaseConfig<MockPlexApiConfig>
{
    public bool SignInResponseIsValid { get; set; } = true;

    public bool UnauthorizedAccessiblePlexServers { get; set; } = false;

    public List<PlexMockServerConfig> MockServers { get; set; } = [];
}
