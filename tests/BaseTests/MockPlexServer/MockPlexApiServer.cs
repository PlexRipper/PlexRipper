using System.Net;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Moq.Contrib.HttpClient;

namespace PlexRipper.BaseTests;

public class MockPlexApiServer : IMockPlexApiServer
{
    private Seed _seed;
    private Action<PlexApiDataConfig> _options;
    private PlexApiDataConfig _config;

    private List<PlexDevice> _servers = [];

    /// <summary>
    /// Key: PlexDevice.ClientIdentifier
    /// </summary>
    private Dictionary<string, List<Connections>> _connections = [];

    /// <summary>
    /// Key: PlexDevice.ClientIdentifier
    /// </summary>
    private Dictionary<string, List<GetAllLibrariesDirectory>> _libraries = [];

    private Dictionary<string, List<GetLibraryItemsMetadata>> _media = [];

    public void Setup(Mock<HttpMessageHandler> handler, Action<PlexApiDataConfig> options)
    {
        _options = options;
        _config = PlexApiDataConfig.FromOptions(_options);
        _seed = _config.Seed;

        SetupServers(handler);

        SetupIdentityRequest(handler);

        SetupLibraries(handler);

        SetupMedia(handler);
    }

    private void SetupServers(Mock<HttpMessageHandler> handler)
    {
        for (var i = 0; i < _config.PlexServerAccessCount; i++)
        {
            var tempSeed = new Seed(_seed.Value);

            var plexServerWithNonHttps = FakePlexApiData.GetServerResource(tempSeed).Generate();
            var plexServerWithHttps = FakePlexApiData
                .GetServerResource(
                    tempSeed,
                    y =>
                    {
                        y.PlexServerAccessConnectionsIncludeHttps = true;
                    }
                )
                .Generate();

            plexServerWithNonHttps.Connections.AddRange(plexServerWithHttps.Connections);

            _connections.Add(plexServerWithNonHttps.ClientIdentifier, plexServerWithNonHttps.Connections);
            _servers.Add(plexServerWithNonHttps);
        }

        var uriBuilder = new UriBuilder("https://plex.tv/") { Path = "/api/v2/resources" };
        handler
            .SetupRequestAnyQuery(HttpMethod.Get, uriBuilder.Uri)
            .ReturnsAsync(
                (HttpRequestMessage req, CancellationToken _) =>
                {
                    var queryDict = req.ParseQueryToDictionary();
                    var includeHttps = false;
                    var includeRelay = false;
                    var includeIPv6 = false;

                    if (queryDict.TryGetValue("includeHttps", out var includeHttpsValue))
                        includeHttps = includeHttpsValue == "1";

                    if (queryDict.TryGetValue("includeRelay", out var includeRelayValue))
                        includeRelay = includeRelayValue == "1";

                    if (queryDict.TryGetValue("includeIPv6", out var includeIPv6Value))
                        includeIPv6 = includeIPv6Value == "1";

                    foreach (var server in _servers)
                    {
                        server.Connections = server
                            .Connections.Where(connection =>
                                connection.Protocol == (includeHttps ? Protocol.Http : Protocol.Https)
                                || connection.Relay == includeRelay
                                || connection.IPv6 == includeIPv6
                            )
                            .ToList();
                    }

                    return FakePlexApiData
                        .GetServerResourcesResponse(HttpStatusCode.OK, _seed, devices: _servers, req)
                        .PlexDevices.ToJsonHttpResponse(req, HttpStatusCode.OK);
                }
            );
    }

    private void SetupIdentityRequest(Mock<HttpMessageHandler> handler)
    {
        foreach (var server in _servers)
        {
            foreach (var connection in _connections[server.ClientIdentifier])
            {
                handler.SetupIdentityRequest(_seed, connection.Uri);
            }
        }
    }

    private void SetupLibraries(Mock<HttpMessageHandler> handler)
    {
        foreach (var server in _servers)
        {
            var libraries = FakePlexApiData
                .GetLibrariesResponseDirectory(_seed, _options)
                .Generate(_config.LibraryCount);
            _libraries.Add(server.ClientIdentifier, libraries);

            foreach (var connection in _connections[server.ClientIdentifier])
            {
                var uriBuilder = new UriBuilder(connection.Uri) { Path = "/library/sections" };
                handler
                    .SetupRequest(HttpMethod.Get, uriBuilder.Uri)
                    .ReturnsAsync(
                        (HttpRequestMessage req, CancellationToken _) =>
                            FakePlexApiData
                                .GetAllLibrariesResponse(
                                    HttpStatusCode.OK,
                                    _seed,
                                    new GetAllLibrariesResponseBody()
                                    {
                                        MediaContainer = new GetAllLibrariesMediaContainer() { Directory = libraries },
                                    },
                                    request: req
                                )
                                .Object.ToJsonHttpResponse(req, HttpStatusCode.OK)
                    );
            }
        }
    }

    private void SetupMedia(Mock<HttpMessageHandler> handler)
    {
        foreach (var server in _servers)
        {
            foreach (var library in _libraries[server.ClientIdentifier])
            {
                // Generate media for each library
                var media = FakePlexApiData
                    .GetLibraryMediaMetadata(_seed, library.Type.ToPlexMediaType(), _options)
                    .Generate(_config.LibraryMetaDataCount);

                _media.Add(library.Key, media);

                foreach (var connection in _connections[server.ClientIdentifier])
                {
                    var uriBuilder = new UriBuilder(connection.Uri) { Path = $"/library/sections/{library.Key}/all" };

                    handler
                        .SetupRequestAnyQuery(HttpMethod.Get, uriBuilder.Uri)
                        .ReturnsAsync(
                            (HttpRequestMessage req, CancellationToken _) =>
                            {
                                var queryDict = req.ParseQueryToDictionary();

                                int containerStart = 0,
                                    containerSize = 0;
                                if (queryDict.TryGetValue("X-Plex-Container-Start", out var containerStartValue))
                                    containerStart = int.Parse(containerStartValue);
                                if (queryDict.TryGetValue("X-Plex-Container-Size", out var containerSizeValue))
                                    containerSize = int.Parse(containerSizeValue);

                                var responseBody = FakePlexApiData.GetPlexLibrarySectionAllResponse(
                                    _seed,
                                    library,
                                    _options
                                );
                                var fullList = _media[library.Key];

                                // Apply slicing based on containerStart and containerSize
                                if (containerSize > 0)
                                {
                                    responseBody.MediaContainer!.Metadata = fullList
                                        .Skip(containerStart)
                                        .Take(containerSize)
                                        .ToList();
                                }
                                else
                                {
                                    // No size specified: return everything from containerStart to the end
                                    responseBody.MediaContainer!.Metadata = fullList.Skip(containerStart).ToList();
                                }

                                return FakePlexApiData
                                    .GetLibraryMediaItemsResponse(
                                        HttpStatusCode.OK,
                                        _seed,
                                        library,
                                        responseBody,
                                        request: req
                                    )
                                    .Object.ToJsonHttpResponse(req, HttpStatusCode.OK);
                            }
                        );
                }
            }
        }
    }
}
