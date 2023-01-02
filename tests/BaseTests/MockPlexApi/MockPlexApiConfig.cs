namespace PlexRipper.BaseTests;

public class MockPlexApiConfig
{
    #region Constructors

    public MockPlexApiConfig() { }

    #endregion

    #region Properties

    public bool SignInResponseIsValid { get; set; } = true;

    public int AccessiblePlexServers { get; set; } = 3;

    public bool UnauthorizedAccessiblePlexServers { get; set; } = false;

    #endregion

    #region Methods

    #region Public

    public static MockPlexApiConfig FromOptions(Action<MockPlexApiConfig> action = null, MockPlexApiConfig defaultValue = null)
    {
        var config = defaultValue ?? new MockPlexApiConfig();
        action?.Invoke(config);
        return config;
    }

    #endregion

    #endregion
}