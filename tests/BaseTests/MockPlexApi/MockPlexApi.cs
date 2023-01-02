using JustEat.HttpClientInterception;

namespace PlexRipper.BaseTests;

/// <summary>
/// Used to mock the responses from the Plex API mainly account authentication.
/// Source: https://github.com/justeat/httpclient-interception
/// </summary>
public class MockPlexApi
{
    #region Fields

    private readonly HttpClientInterceptorOptions _clientOptions;
    private readonly MockPlexApiConfig _config;

    #endregion

    #region Constructors

    public MockPlexApi([CanBeNull] Action<MockPlexApiConfig> options = null)
    {
        _config = MockPlexApiConfig.FromOptions(options);

        _clientOptions = new HttpClientInterceptorOptions
        {
            ThrowOnMissingRegistration = true,
        };

        Setup();
    }

    #endregion

    #region Methods

    #region Private

    private void Setup()
    {
        BaseRequest().SetupSignIn(_config).RegisterWith(_clientOptions);
        BaseRequest().SetupServerResources(_config).RegisterWith(_clientOptions);
    }

    private static HttpRequestInterceptionBuilder BaseRequest()
    {
        return new HttpRequestInterceptionBuilder()
            .Requests()
            .ForUri(new Uri("https://plex.tv"));
    }

    #endregion

    #region Public

    public System.Net.Http.HttpClient CreateClient()
    {
        return _clientOptions.CreateHttpClient();
    }

    #endregion

    #endregion
}