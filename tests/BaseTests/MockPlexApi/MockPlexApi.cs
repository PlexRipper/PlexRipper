using System.Net.Http.Headers;
using JustEat.HttpClientInterception;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

/// <summary>
/// Used to mock the responses from the Plex API
/// Source: https://github.com/justeat/httpclient-interception
/// </summary>
public class MockPlexApi
{
    #region Fields

    private readonly HttpClientInterceptorOptions _clientOptions;
    private readonly MockPlexApiConfig _config;
    private readonly PlexApiDataConfig _fakeDataConfig;
    private readonly List<Uri> _serverUris = new();
    private readonly System.Net.Http.HttpClient _client = new();

    #endregion

    #region Constructors

    public MockPlexApi(Action<MockPlexApiConfig> options = null, List<Uri> serverUris = null)
    {
        _serverUris = serverUris;
        _config = MockPlexApiConfig.FromOptions(options);
        _fakeDataConfig = _config.FakeDataConfig;

        _clientOptions = new HttpClientInterceptorOptions
        {
            // TODO https://github.com/justeat/httpclient-interception/issues/510
            ThrowOnMissingRegistration = true,
            OnMissingRegistration = message =>
            {
                if (message.RequestUri != null && message.RequestUri.Host == "localhost")
                {
                    return _client.SendAsync(new HttpRequestMessage
                    {
                        RequestUri = message.RequestUri,
                        Method = message.Method,
                        Content = message.Content,
                        Headers =
                        {
                            UserAgent =
                            {
                                new ProductInfoHeaderValue(
                                    new ProductHeaderValue(nameof(MockPlexApi), "1.0")),
                            },
                            Range = message.Headers.Range,
                        },
                    });
                }

                Log.Error($"OnMissingRegistration was triggered on uri: {message.RequestUri} and not handled");
                return null;
            },
        };

        Setup();
        Log.Debug($"{nameof(MockPlexApi)} was set-up");
    }

    #endregion

    #region Methods

    #region Private

    private void Setup()
    {
        SetupSignIn(_config);
        SetupServerResources(_config);
        SetupTimeOutConnections();
    }

    private static HttpRequestInterceptionBuilder BaseRequest()
    {
        return new HttpRequestInterceptionBuilder()
            .Requests()
            .ForHttps()
            .ForHost(PlexApiPaths.Host);
    }

    private void SetupServerResources(MockPlexApiConfig config)
    {
        var query = BaseRequest()
            .ForGet()
            .ForPath(PlexApiPaths.ServerResourcesPath.TrimStart('/'))
            .IgnoringQuery();

        if (!config.UnauthorizedAccessiblePlexServers)
        {
            var servers = FakePlexApiData
                .GetServerResource()
                .Generate(config.AccessiblePlexServers);

            for (var i = 0; i < _serverUris.Count; i++)
            {
                var uri = _serverUris[i];
                if (i < servers.Count)
                {
                    servers[i].Connections[0].Uri = uri.ToString();
                    servers[i].Connections[0].Protocol = uri.Scheme;
                    servers[i].Connections[0].Address = uri.Host;
                    servers[i].Connections[0].Port = uri.Port;
                    servers[i].Connections[0].Local = true;
                }
            }

            query.Responds()
                .WithStatus(200)
                .WithJsonContent(servers);
        }
        else
        {
            query.Responds()
                .WithStatus(401)
                .WithJsonContent(FakePlexApiData.GetFailedServerResourceResponse());
        }

        query.RegisterWith(_clientOptions);
    }

    private void SetupSignIn(MockPlexApiConfig config)
    {
        var query = BaseRequest()
            .ForPost()
            .ForPath(PlexApiPaths.SignInPath)
            .IgnoringQuery();

        if (config.SignInResponseIsValid)
        {
            query.Responds()
                .WithStatus(201) // 201 is correct here, Plex for some reason gives this back on this request
                .WithJsonContent(FakePlexApiData.GetPlexSignInResponse().Generate());
        }
        else
        {
            query.Responds()
                .WithStatus(401)
                .WithJsonContent(FakePlexApiData.GetFailedPlexSignInResponse());
        }

        query.RegisterWith(_clientOptions);
    }

    private void SetupTimeOutConnections()
    {
        var builder = new HttpRequestInterceptionBuilder()
            .ForGet()
            .ForHttp()
            .ForHost("240.0.0.0")
            .ForPort(-1)
            .IgnoringPath()
            .IgnoringQuery()
            .WithInterceptionCallback((_) => Task.Delay(2000));

        _clientOptions.Register(builder);
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