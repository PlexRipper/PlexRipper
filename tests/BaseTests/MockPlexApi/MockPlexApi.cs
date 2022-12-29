using JustEat.HttpClientInterception;
using PlexRipper.PlexApi.Common;

namespace PlexRipper.BaseTests;

/// <summary>
/// Used to mock the responses from the Plex API mainly account authentication.
/// Source: https://github.com/justeat/httpclient-interception
/// </summary>
public class MockPlexApi
{
    private readonly MockPlexApiConfig _config;
    private readonly HttpClientInterceptorOptions _clientOptions;
    private readonly HttpRequestInterceptionBuilder _builder;

    public MockPlexApi([CanBeNull] Action<MockPlexApiConfig> options = null)
    {
        _config = MockPlexApiConfig.FromOptions(options);

        _clientOptions = new HttpClientInterceptorOptions();
        _builder = new HttpRequestInterceptionBuilder();

        _clientOptions.ThrowOnMissingRegistration = true;

        SetupSignIn();
    }


    public System.Net.Http.HttpClient CreateClient()
    {
        return _clientOptions.CreateHttpClient();
    }

    private void SetupSignIn()
    {
        var query = _builder
            .Requests()
            .ForPost()
            .ForHttps()
            .ForHost(PlexApiPaths.Host)
            .ForPath(PlexApiPaths.SignInPath)
            .IgnoringQuery();

        if (_config.SignInResponseIsValid)
        {
            query.Responds()
                .WithStatus(201)
                .WithJsonContent(FakePlexApiData.FakePlexApiData.GetPlexSignInResponse().Generate());
        }
        else
        {
            query.Responds()
                .WithStatus(401)
                .WithJsonContent(FakePlexApiData.FakePlexApiData.GetFailedPlexSignInResponse());
        }

        query.RegisterWith(_clientOptions);
    }
}