using System.Net.Http.Headers;
using JustEat.HttpClientInterception;
using Logging.Interface;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

/// <summary>
/// Used to mock the responses from the Plex API
/// Source: https://github.com/justeat/httpclient-interception
/// </summary>
public class MockPlexApi
{
    private readonly HttpClientInterceptorOptions _clientOptions;
    private readonly MockPlexApiConfig _config;
    private readonly List<Uri> _serverUris;
    private readonly HttpClient _client = new();

    public MockPlexApi(ILog log, Action<MockPlexApiConfig>? options = null, List<Uri>? serverUris = null)
    {
        _serverUris = serverUris ?? [];
        _config = MockPlexApiConfig.FromOptions(options);

        _clientOptions = new HttpClientInterceptorOptions
        {
            // TODO https://github.com/justeat/httpclient-interception/issues/510
            ThrowOnMissingRegistration = true,

            OnMissingRegistration = message =>
            {
                if (message.RequestUri != null && message.RequestUri.Host == "localhost")
                {
                    return _client.SendAsync(
                        new HttpRequestMessage
                        {
                            RequestUri = message.RequestUri,
                            Method = message.Method,
                            Content = message.Content,
                            Headers =
                            {
                                UserAgent =
                                {
                                    new ProductInfoHeaderValue(new ProductHeaderValue(nameof(MockPlexApi), "1.0")),
                                },
                                Range = message.Headers.Range,
                            },
                        }
                    );
                }

                log.Error(
                    "OnMissingRegistration was triggered on uri: {RequestUri} and not handled",
                    message.RequestUri
                );
                throw new Exception(
                    $"OnMissingRegistration was triggered on uri: {message.RequestUri} and not handled");
            },
        };
        Setup();
        log.Debug("{NameOfMockPlexApi} was set-up", nameof(MockPlexApi));
    }

    private void Setup()
    {
        SetupSignIn(_config);
        SetupServerResources(_config);
        SetupTimeOutConnections();
    }

    private static HttpRequestInterceptionBuilder BaseRequest() =>
        new HttpRequestInterceptionBuilder().Requests().ForHttps().ForHost(PlexApiPaths.Host);

    private void SetupServerResources(MockPlexApiConfig config)
    {
        var query = BaseRequest().ForGet().ForPath(PlexApiPaths.ServerResourcesPath.TrimStart('/')).IgnoringQuery();

        if (!config.UnauthorizedAccessiblePlexServers)
        {
            var servers = FakePlexApiData.GetServerResource().Generate(_serverUris.Count);

            for (var i = 0; i < _serverUris.Count; i++)
                servers[i].Connections =
                [
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = _serverUris[i].Host,
                        Port = _serverUris[i].Port,
                        Uri = _serverUris[i].ToString(),
                        Local = true,
                        Relay = false,
                        IPv6 = false,
                    },
                ];

            query.Responds().WithStatus(200).WithPlexSdkJsonContent(servers);
        }
        else
            query.Responds().WithStatus(401).WithPlexSdkJsonContent(FakePlexApiData.GetFailedServerResourceResponse());

        query.RegisterWith(_clientOptions);
    }

    private void SetupSignIn(MockPlexApiConfig config)
    {
        var query = BaseRequest().ForPost().ForPath(PlexApiPaths.SignInPath).IgnoringQuery();

        if (config.SignInResponseIsValid)
        {
            query
                .Responds()
                .WithStatus(201) // 201 is correct here, Plex for some reason gives this back on this request
                .WithPlexSdkJsonContent(FakePlexApiData.GetPlexSignInResponse().Generate());
        }
        else
            query.Responds().WithStatus(401).WithPlexSdkJsonContent(FakePlexApiData.GetFailedPlexSignInResponse());

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

    public HttpClient CreateClient()
    {
        var client = _clientOptions.CreateHttpClient();
        client.DefaultRequestHeaders.Add("MockPlexApi", "true");
        return client;
    }
}