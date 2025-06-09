using System.Net;
using Data.Contracts;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Moq.Contrib.HttpClient;
using PlexRipper.PlexApi;

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
    /// Key: GetAllLibrariesDirectory.Uuid (library Uuid)
    /// </summary>
    private readonly Dictionary<string, List<GetMediaMetaDataMetadata>> _movies = [];

    /// <summary>
    /// Key: GetAllLibrariesDirectory.Uuid (library Uuid)
    /// </summary>

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<GetMediaMetaDataMetadata>> _tvShows = [];

    /// <summary>
    /// Key: GetAllLibrariesDirectory.Uuid (library Uuid)
    /// </summary>

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<GetMediaMetaDataMetadata>> _seasons = [];

    /// <summary>
    /// Key: GetAllLibrariesDirectory.Uuid (library Uuid)
    /// </summary>

    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<GetMediaMetaDataMetadata>> _episodes = [];

    private IPlexRipperDbContext _dbContext;

    public MockPlexApiServer(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
        var devices = new List<PlexDevice>();

        // If we are generating from the database, fetch existing servers
        if (_config.GenerateFromDatabase)
        {
            var plexApiDTOList = _dbContext.PlexServers.IncludeConnections().ToList().ToPlexApiDTO();
            devices.AddRange(plexApiDTOList);
        }

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

            devices.Add(plexServerWithNonHttps);
        }

        // Add devices and connections to the internal lists
        foreach (var device in devices)
        {
            _connections.Add(device.ClientIdentifier, device.Connections);
            _servers.Add(device);
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
        // If we are generating from the database, fetch existing libraries
        if (_config.GenerateFromDatabase)
        {
            var plexServers = _dbContext.PlexServers.IncludeLibraries().ToList();

            foreach (var plexServer in plexServers)
                _libraries.Add(plexServer.MachineIdentifier, plexServer.PlexLibraries.ToList().ToPlexApiDTO());
        }

        var libraries = new List<GetAllLibrariesDirectory>();
        foreach (var server in _servers)
        {
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

            _libraries.TryAdd(server.ClientIdentifier, libraries);
        }

        // Setup libraries responses
        foreach (var server in _servers)
        {
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
                var libraryKey = library.Uuid;
                if (type == PlexMediaType.Movie)
                {
                    var movies = FakePlexApiData
                        .GetMediaMetaDataMetadata(_seed, PlexMediaType.Movie, _options)
                        .Generate(_config.MoviesPerLibraryCount);

                    _movies.Add(libraryKey, movies);
                }
                else if (type == PlexMediaType.TvShow)
                {
                    var tvShows = FakePlexApiData
                        .GetMediaMetaDataMetadata(_seed, PlexMediaType.TvShow, _options)
                        .Generate(_config.TvShowsPerLibraryCount);

                    _tvShows.Add(libraryKey, tvShows);

                    foreach (var tvShow in tvShows)
                    {
                        var seasons = FakePlexApiData
                            .GetMediaMetaDataMetadata(_seed, PlexMediaType.Season, _options)
                            .Generate(_config.SeasonsPerTvShowCount);

                        seasons.ForEach(x => x.SetParentValues(tvShow));

                        _seasons.Add(libraryKey, seasons);

                        foreach (var season in seasons)
                        {
                            var episodes = FakePlexApiData
                                .GetMediaMetaDataMetadata(_seed, PlexMediaType.Episode, _options)
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

                                // Check if the type is specified in the query parameters
                                var libraryType = PlexMediaType.Unknown;
                                if (queryDict.TryGetValue("type", out var type))
                                    libraryType = type.ToPlexMediaTypeFromTypeInt();

                                var responseBody = FakePlexApiData.GetPlexLibrarySectionAllResponse(
                                    _seed,
                                    library,
                                    options: _options
                                );

                                var fullList = libraryType switch
                                {
                                    PlexMediaType.Movie => _movies.TryGetValue(library.Uuid, out var list) ? list : [],
                                    PlexMediaType.TvShow => _tvShows.TryGetValue(library.Uuid, out var list)
                                        ? list
                                        : [],
                                    PlexMediaType.Season => _seasons.TryGetValue(library.Uuid, out var list)
                                        ? list
                                        : [],
                                    PlexMediaType.Episode => _episodes.TryGetValue(library.Uuid, out var list)
                                        ? list
                                        : [],
                                    _ => throw new ArgumentOutOfRangeException(
                                        nameof(libraryType),
                                        $"Unhandled library type: {libraryType}"
                                    ),
                                };

                                // Apply slicing based on containerStart and containerSize
                                if (containerSize > 0)
                                {
                                    responseBody.MediaContainer!.Metadata = fullList
                                        .Skip(containerStart)
                                        .Take(containerSize)
                                        .Select(x => x.ToLibraryItemsMetadata())
                                        .ToList();
                                }
                                else
                                {
                                    // No size specified: return everything from containerStart to the end
                                    responseBody.MediaContainer!.Metadata = fullList
                                        .Skip(containerStart)
                                        .Select(x => x.ToLibraryItemsMetadata())
                                        .ToList();
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

                    // Mock the metadata retrieval for specific items
                    handler
                        .SetupRequest(
                            HttpMethod.Get,
                            uri => uri.RequestUri!.AbsolutePath.StartsWith("/library/metadata/")
                        )
                        .ReturnsAsync(
                            (HttpRequestMessage request, CancellationToken _) =>
                            {
                                var metadataIds = request
                                    .RequestUri!.AbsolutePath.Split("/library/metadata/")[1]
                                    .Split(',')
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Select(int.Parse)
                                    .ToArray();

                                var metadataList = metadataIds
                                    .Select(x => GetMediaItem(x.ToString()))
                                    .Where(x => x != null)
                                    .ToList();

                                var responseBody = FakePlexApiData.GetMediaMetaDataResponseBodyResponse(
                                    _seed,
                                    library,
                                    options: _options
                                );

                                responseBody.MediaContainer!.Metadata = metadataList;
                                responseBody.MediaContainer!.Size = metadataList.Count;

                                return FakePlexApiData
                                    .GetMediaMetaDataAsync(HttpStatusCode.OK, _seed, library, responseBody, request)
                                    .Object.ToJsonHttpResponse(request, HttpStatusCode.OK);
                            }
                        );
                }
            }
        }
    }

    private GetMediaMetaDataMetadata? GetMediaItem(string key)
    {
        foreach (var movie in _movies)
        {
            var result = movie.Value.FirstOrDefault(x => x.RatingKey == key);
            if (result != null)
                return result;
        }

        foreach (var episode in _episodes)
        {
            var result = episode.Value.FirstOrDefault(x => x.RatingKey == key);
            if (result != null)
                return result;
        }

        foreach (var tvShow in _tvShows)
        {
            var result = tvShow.Value.FirstOrDefault(x => x.RatingKey == key);
            if (result != null)
                return result;
        }

        foreach (var season in _seasons)
        {
            var result = season.Value.FirstOrDefault(x => x.RatingKey == key);
            if (result != null)
                return result;
        }

        return null;
    }
}
