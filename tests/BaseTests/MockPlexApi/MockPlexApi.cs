using JustEat.HttpClientInterception;
using PlexRipper.PlexApi.Common;

namespace PlexRipper.BaseTests;

/// <summary>
/// Used to mock the responses from the Plex API mainly account authentication.
/// Source: https://github.com/justeat/httpclient-interception
/// </summary>
public class MockPlexApi
{
    private readonly List<Uri> _serverUris = new();

    #region Fields

    private readonly HttpClientInterceptorOptions _clientOptions;
    private readonly MockPlexApiConfig _config;

    #endregion

    #region Constructors

    public MockPlexApi([CanBeNull] Action<MockPlexApiConfig> options = null, List<Uri> serverUris = null)
    {
        _serverUris = serverUris;
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
        SetupServerResources(_config);
        SetupPlexServers();
    }

    public void SetupServerResources(MockPlexApiConfig config)
    {
        var query = BaseRequest()
            .ForGet()
            .ForPath(PlexApiPaths.ServerResourcesPath)
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

    private void SetupPlexServers()
    {
        new HttpRequestInterceptionBuilder()
            .Requests()
            .ForHttp()
            .ForHost("localhost")
            .ForPort(-1)
            .IgnoringPath()
            .IgnoringQuery()
            .Responds()
            .WithStatus(200)
            .WithJsonContent("{x: true}")
            .RegisterWith(_clientOptions);
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