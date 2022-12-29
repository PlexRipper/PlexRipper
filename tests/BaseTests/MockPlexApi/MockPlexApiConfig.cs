namespace PlexRipper.BaseTests;

public class MockPlexApiConfig
{
    public MockPlexApiConfig() { }

    public static MockPlexApiConfig FromOptions(Action<MockPlexApiConfig> action = null, MockPlexApiConfig defaultValue = null)
    {
        var config = defaultValue ?? new MockPlexApiConfig();
        action?.Invoke(config);
        return config;
    }

    public bool SignInResponseIsValid { get; set; } = true;
}