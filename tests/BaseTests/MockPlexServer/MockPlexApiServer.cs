using System.Net;
using LukeHagar.PlexAPI.SDK.Models.Components;
using Moq.Contrib.HttpClient;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.BaseTests;

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
    private readonly Dictionary<string, List<LibrarySection>> _libraries = [];

    /// <summary>
    /// Key: LibrarySection.Uuid (library Uuid)
    /// </summary>
    private readonly Dictionary<string, List<Metadata>> _movies = [];

    /// <summary>
    /// Key: LibrarySection.Uuid (library Uuid)
    /// </summary>
    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<Metadata>> _tvShows = [];

    /// <summary>
    /// Key: LibrarySection.Uuid (library Uuid)
    /// </summary>
    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<Metadata>> _seasons = [];

    /// <summary>
    /// Key: LibrarySection.Uuid (library Uuid)
    /// </summary>
    // ReSharper disable once CollectionNeverQueried.Local
    private readonly Dictionary<string, List<Metadata>> _episodes = [];

    private IReaparrDbContext _dbContext;

    public MockPlexApiServer(IReaparrDbContext dbContext)
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
            _connections.TryAdd(device.ClientIdentifier, device.Connections);

            if (_servers.All(s => s.ClientIdentifier != device.ClientIdentifier))
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
                                connection.Protocol
                                    == (includeHttps ? PlexDeviceProtocol.Https : PlexDeviceProtocol.Http)
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
                _libraries.TryAdd(plexServer.MachineIdentifier, plexServer.PlexLibraries.ToList().ToPlexApiDTO());
        }

        var libraries = new List<LibrarySection>();
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
                var uriBuilder = new UriBuilder(connection.Uri) { Path = "/library/sections/all" };
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
                var type = library.Type.ToPlexMediaType();
                var libraryKey = library.Uuid;

                if (type == PlexMediaType.Movie)
                {
                    var movies = FakePlexApiData
                        .GetMediaMetaDataMetadata(_seed, PlexMediaType.Movie, _options)
                        .Generate(_config.MoviesPerLibraryCount);

                    _movies.TryAdd(libraryKey, movies);
                    continue;
                }

                if (type == PlexMediaType.TvShow)
                {
                    var tvShowList = new List<Metadata>();
                    var seasonList = new List<Metadata>();
                    var episodeList = new List<Metadata>();

                    var tvShows = FakePlexApiData
                        .GetMediaMetaDataMetadata(_seed, PlexMediaType.TvShow, _options)
                        .Generate(_config.TvShowsPerLibraryCount);

                    tvShowList.AddRange(tvShows);
                    foreach (var tvShow in tvShows)
                    {
                        var seasons = FakePlexApiData
                            .GetMediaMetaDataMetadata(_seed, PlexMediaType.Season, _options)
                            .Generate(_config.SeasonsPerTvShowCount);

                        seasons.ForEach(x => x.SetParentValues(tvShow));

                        seasonList.AddRange(seasons);
                        foreach (var season in seasons)
                        {
                            var episodes = FakePlexApiData
                                .GetMediaMetaDataMetadata(_seed, PlexMediaType.Episode, _options)
                                .Generate(_config.EpisodesPerSeasonCount);

                            episodes.ForEach(x => x.SetParentValues(season));
                            episodes.ForEach(x => x.SetGrandparentValues(tvShow));
                            episodeList.AddRange(episodes);
                        }
                    }

                    _tvShows.TryAdd(libraryKey, tvShowList);
                    _seasons.TryAdd(libraryKey, seasonList);
                    _episodes.TryAdd(libraryKey, episodeList);
                    continue;
                }

                throw new InvalidOperationException($"Unsupported library type: {library.Type}");
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

                                if (
                                    queryDict.TryGetValue("X-Plex-Container-Start", out var containerStartValue)
                                    && int.TryParse(containerStartValue, out var start)
                                )
                                    containerStart = start;

                                if (
                                    queryDict.TryGetValue("X-Plex-Container-Size", out var containerSizeValue)
                                    && int.TryParse(containerSizeValue, out var size)
                                )
                                    containerSize = size;

                                // Check if the type is specified in the query parameters
                                var libraryType = PlexMediaType.Unknown;
                                if (queryDict.TryGetValue("type", out var type))
                                    libraryType = type.ToPlexMediaTypeFromTypeInt();

                                var responseBody = FakePlexApiData.GetLibrarySectionsAllResponseBody(
                                    _seed,
                                    library,
                                    options: _options
                                );

                                var fullList = GetMediaItems(library.Uuid, libraryType);

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

                                responseBody.MediaContainer!.Size = responseBody.MediaContainer.Metadata!.Count;
                                responseBody.MediaContainer!.TotalSize = fullList.Count;

                                return FakePlexApiData
                                    .GetLibrarySectionsAllResponse(
                                        HttpStatusCode.OK,
                                        _seed,
                                        library,
                                        responseBody,
                                        request: req
                                    )
                                    .MediaContainerWithMetadata!.ToJsonHttpResponse(req, HttpStatusCode.OK);
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
                                var urlEncodedIds = request.RequestUri!.AbsolutePath.Split("/library/metadata/")[1];
                                var metadataIds = Uri.UnescapeDataString(urlEncodedIds)
                                    .Split(',')
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Select(int.Parse)
                                    .ToArray();

                                var metadataList = metadataIds
                                    .Select(x => GetMediaItem(x.ToString()))
                                    .Where(x => x is not null)
                                    .Select(x => x!)
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
                                    .MediaContainerWithMetadata!.ToJsonHttpResponse(request, HttpStatusCode.OK);
                            }
                        );
                }
            }
        }
    }

    private Metadata? GetMediaItem(string key)
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

    private ICollection<Metadata> GetMediaItems(string libraryUuid, PlexMediaType libraryType)
    {
        return libraryType switch
        {
            PlexMediaType.Movie => _movies.TryGetValue(libraryUuid, out var list) ? list : [],
            PlexMediaType.TvShow => _tvShows.TryGetValue(libraryUuid, out var list) ? list : [],
            PlexMediaType.Season => _seasons.TryGetValue(libraryUuid, out var list) ? list : [],
            PlexMediaType.Episode => _episodes.TryGetValue(libraryUuid, out var list) ? list : [],
            _ => throw new ArgumentOutOfRangeException(nameof(libraryType), $"Unhandled library type: {libraryType}"),
        };
    }
}
