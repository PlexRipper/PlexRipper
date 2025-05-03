using System.Net;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Moq.Contrib.HttpClient;

namespace PlexRipper.BaseTests;

public class MockPlexApiServer : IMockPlexApiServer
{
    private Seed _seed = new(1);
    private Action<PlexApiDataConfig> _options = null!;
    private PlexApiDataConfig _config = null!;

    private readonly List<PlexDevice> _servers = [];

    /// <summary>
    /// Key: PlexDevice.ClientIdentifier (server key)
    /// </summary>
    private readonly Dictionary<string, List<Connections>> _connections = [];

    /// <summary>
    /// Key: PlexDevice.ClientIdentifier (server key)
    /// </summary>
    private readonly Dictionary<string, List<GetAllLibrariesDirectory>> _libraries = [];

    /// <summary>
    /// Key: GetAllLibrariesDirectory.Key (library key)
    /// </summary>
    private readonly Dictionary<string, List<GetLibraryItemsMetadata>> _movies = [];

    /// <summary>
    /// Key: GetAllLibrariesDirectory.Key (library key)
    /// </summary>

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<GetLibraryItemsMetadata>> _tvShows = [];

    /// <summary>
    /// Key: GetAllLibrariesDirectory.Key (library key)
    /// </summary>

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<GetLibraryItemsMetadata>> _seasons = [];

    /// <summary>
    /// Key: GetAllLibrariesDirectory.Key (library key)
    /// </summary>

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<GetLibraryItemsMetadata>> _episodes = [];

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
            var tempSeed = new Seed(_seed.Next());

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
                    if (_config.SetServerResourcesResponse == HttpStatusCode.Unauthorized)
                    {
                        return FakePlexApiData
                            .GetPlexUnauthorizedResponseMessage(req)
                            .ToJsonHttpResponse(req, HttpStatusCode.Unauthorized);
                    }

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
            var libraries = new List<GetAllLibrariesDirectory>();
            if (_config.MovieLibraryCount > 0)
            {
                libraries.AddRange(
                    FakePlexApiData
                        .GetLibrariesResponseDirectory(_seed, PlexMediaType.Movie)
                        .Generate(_config.MovieLibraryCount)
                );
            }

            if (_config.TvShowLibraryCount > 0)
            {
                libraries.AddRange(
                    FakePlexApiData
                        .GetLibrariesResponseDirectory(_seed, PlexMediaType.TvShow)
                        .Generate(_config.TvShowLibraryCount)
                );
            }

            _libraries.Add(server.ClientIdentifier, libraries);

            foreach (var connection in _connections[server.ClientIdentifier])
            {
                var uriBuilder = new UriBuilder(connection.Uri) { Path = "/library/sections" };
                handler
                    .SetupRequest(HttpMethod.Get, uriBuilder.Uri)
                    .ReturnsAsync(
                        (HttpRequestMessage req, CancellationToken _) =>
                        {
                            var response = FakePlexApiData.GetAllLibrariesResponse(
                                HttpStatusCode.OK,
                                _seed,
                                request: req
                            );
                            response.Object.ShouldNotBeNull();
                            response.Object.MediaContainer.ShouldNotBeNull();
                            response.Object.MediaContainer.Directory = _libraries[server.ClientIdentifier];

                            return response.Object.ToJsonHttpResponse(req, HttpStatusCode.OK);
                        }
                    );
            }
        }
    }

    private void SetupMedia(Mock<HttpMessageHandler> handler)
    {
        foreach (var server in _servers)
        {
            // Generate media for each library
            foreach (var library in _libraries[server.ClientIdentifier])
            {
                var type = library.Type.ToString().ToPlexMediaType();
                var libraryKey = library.Key;
                if (type == PlexMediaType.Movie)
                {
                    var movies = FakePlexApiData
                        .GetLibraryMediaMetadata(_seed, PlexMediaType.Movie, _options)
                        .Generate(_config.MoviesPerLibraryCount);

                    _movies.Add(libraryKey, movies);
                }
                else if (type == PlexMediaType.TvShow)
                {
                    var tvShows = FakePlexApiData
                        .GetLibraryMediaMetadata(_seed, PlexMediaType.TvShow, _options)
                        .Generate(_config.TvShowsPerLibraryCount);

                    _tvShows.Add(libraryKey, tvShows);

                    foreach (var tvShow in tvShows)
                    {
                        var seasons = FakePlexApiData
                            .GetLibraryMediaMetadata(_seed, PlexMediaType.Season, _options)
                            .Generate(_config.SeasonsPerTvShowCount);

                        seasons.ForEach(x => x.SetParentValues(tvShow));

                        _seasons.Add(libraryKey, seasons);

                        foreach (var season in seasons)
                        {
                            var episodes = FakePlexApiData
                                .GetLibraryMediaMetadata(_seed, PlexMediaType.Episode, _options)
                                .Generate(_config.EpisodesPerSeasonCount);

                            episodes.ForEach(x => x.SetParentValues(season));
                            episodes.ForEach(x => x.SetGrandparentValues(tvShow));

                            _episodes.Add(libraryKey, episodes);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported library type: {library.Type}");
                }
            }
        }

        // Setup media responses
        foreach (var server in _servers)
        {
            foreach (var library in _libraries[server.ClientIdentifier])
            {
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
                                    options: _options
                                );

                                var fullList = _movies[library.Key];

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
