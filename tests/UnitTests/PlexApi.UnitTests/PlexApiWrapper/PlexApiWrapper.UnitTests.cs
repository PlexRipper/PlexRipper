using System.Net;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;
using PlexRipper.PlexApi;

namespace PlexApi.UnitTests;

public class PlexApiWrapperUnitTests : BaseUnitTest<PlexApiWrapper>
{
    public PlexApiWrapperUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnEmptyResult_WhenAuthTokenIsEmpty()
    {
        // Act
        var result = await _sut.GetAccessibleServers(string.Empty);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has400BadRequestError().ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("authToken"));
    }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenBothResponsesFail()
    {
        // Arrange
        mock.Mock<IPlexApiClient>()
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                    FakePlexApiData.GetHttpResponseMessage<string?>(HttpStatusCode.InternalServerError, null, request)
            );

        // Act
        var result = await _sut.GetAccessibleServers("FAKE_TOKEN");

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldRemoveDuplicateConnections_WhenMultipleServersHaveTheSameConnection()
    {
        // Arrange
        var serverCount = 5;
        var response1 = FakePlexApiData.GetServerResourcesResponse(
            HttpStatusCode.OK,
            new Seed(939),
            options: config =>
            {
                config.PlexServerAccessCount = serverCount;
            }
        );

        var response2 = FakePlexApiData.GetServerResourcesResponse(
            HttpStatusCode.OK,
            new Seed(939),
            options: config =>
            {
                config.PlexServerAccessCount = serverCount;
                config.PlexServerAccessConnectionsIncludeHttps = true;
            }
        );

        response1.PlexDevices.ShouldNotBeNull();

        var testConnection = response1.PlexDevices[0].Connections[0];

        testConnection.ShouldNotBeNull();
        foreach (var plexDevice in response1.PlexDevices)
        {
            plexDevice.Connections.Add(testConnection);
        }

        mock.Mock<IPlexApiClient>()
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                    !request.RequestUri!.Query.Contains("includeHttps=1")
                        ? FakePlexApiData.GetHttpResponseMessage(HttpStatusCode.OK, response1.PlexDevices, request)
                        : FakePlexApiData.GetHttpResponseMessage(HttpStatusCode.OK, response2.PlexDevices, request)
            );

        // Act
        var result = await _sut.GetAccessibleServers("FAKE_TOKEN");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(serverCount);

        var connections = result.Value.SelectMany(x => x.Connections).ToList();

        connections.Select(connection => connection.Uri).ShouldBeUnique();
    }

    [Fact]
    public async Task ShouldRemoveDuplicateConnectionsFromRealResponse_WhenMultipleServersHaveTheSameConnection()
    {
        // Arrange
        mock.Mock<IPlexApiClient>()
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                {
                    if (request.RequestUri!.Query.Contains("includeHttps=1"))
                    {
                        return new HttpResponseMessage
                        {
                            Content = PlexApiWrapperTestData.Response2.ToStringContent(),
                            ReasonPhrase = HttpStatusCode.OK.ToString(),
                            RequestMessage = request,
                            StatusCode = HttpStatusCode.OK,
                            Version = new Version(1, 1),
                        };
                    }

                    return new HttpResponseMessage
                    {
                        Content = PlexApiWrapperTestData.Response1.ToStringContent(),
                        ReasonPhrase = HttpStatusCode.OK.ToString(),
                        RequestMessage = request,
                        StatusCode = HttpStatusCode.OK,
                        Version = new Version(1, 1),
                    };
                }
            );

        // Act
        var result = await _sut.GetAccessibleServers("FAKE_TOKEN");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var connections = result.Value.SelectMany(x => x.Connections);

        connections.Select(connection => connection.Uri).ShouldBeUnique();
    }

    [Fact]
    public async Task ShouldReturnServersFromSecondResponse_WhenFirstResponseFails()
    {
        // Arrange
        var serverCount = 2;

        mock.Mock<IPlexApiClient>()
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                    !request.RequestUri!.Query.Contains("includeHttps=1")
                        ? FakePlexApiData.GetHttpResponseMessage<string?>(
                            HttpStatusCode.InternalServerError,
                            null,
                            request
                        )
                        : FakePlexApiData
                            .GetServerResourcesResponse(
                                HttpStatusCode.OK,
                                new Seed(939),
                                options: config =>
                                {
                                    config.PlexServerAccessCount = serverCount;
                                    config.PlexServerAccessConnectionsIncludeHttps = true;
                                }
                            )
                            .RawResponse
            );

        // Act
        var result = await _sut.GetAccessibleServers("FAKE_TOKEN");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(serverCount);
    }

    [Fact]
    public async Task ShouldCombineConnectionsFromBothResponses_WhenBothAreSuccessful()
    {
        // Arrange
        var serverCount = 5;

        mock.Mock<IPlexApiClient>()
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                    !request.RequestUri!.Query.Contains("includeHttps=1")
                        ? FakePlexApiData
                            .GetServerResourcesResponse(
                                HttpStatusCode.OK,
                                new Seed(939),
                                options: config =>
                                {
                                    config.PlexServerAccessCount = serverCount;
                                }
                            )
                            .RawResponse
                        : FakePlexApiData
                            .GetServerResourcesResponse(
                                HttpStatusCode.OK,
                                new Seed(940),
                                options: config =>
                                {
                                    config.PlexServerAccessCount = serverCount;
                                    config.PlexServerAccessConnectionsIncludeHttps = true;
                                }
                            )
                            .RawResponse
            );

        // Act
        var result = await _sut.GetAccessibleServers("FAKE_TOKEN");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(serverCount);

        var allConnections = result.Value.SelectMany(x => x.Connections).ToList();
        allConnections.ShouldNotBeEmpty();
        allConnections.Select(c => c.Uri).ShouldBeUnique();
    }

    [Fact]
    public async Task ShouldCombineAllConnectionsFromBothResponses_WhenBothAreSuccessful()
    {
        // Arrange
        var serverResource1 = FakePlexApiData.GetServerResource(new Seed(939)).Generate(2);
        serverResource1[0].Connections.Clear();
        serverResource1[0]
            .Connections.AddRange(
                [
                    new Connections
                    {
                        Protocol = Protocol.Http,
                        Address = "192.168.200.95",
                        Port = 32400,
                        Uri = "http://192.168.200.95:32400",
                        Local = true,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "www.albertflix.nl",
                        Port = 43324,
                        Uri = "https://www.albertflix.nl:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Http,
                        Address = "77.170.188.28",
                        Port = 43324,
                        Uri = "http://77.170.188.28:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                ]
            );
        serverResource1[1].Provides = "client,player";
        serverResource1[1].Connections.Clear();
        serverResource1[1]
            .Connections.AddRange(
                [
                    new Connections
                    {
                        Protocol = Protocol.Http,
                        Address = "192.168.200.95",
                        Port = 32400,
                        Uri = "http://192.168.200.95:32400",
                        Local = true,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "www.albertflix.nl",
                        Port = 43324,
                        Uri = "https://www.albertflix.nl:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Http,
                        Address = "77.170.188.28",
                        Port = 43324,
                        Uri = "http://77.170.188.28:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                ]
            );

        var serverResource2 = FakePlexApiData.GetServerResource(new Seed(939)).Generate(2);
        serverResource2[0].Connections.Clear();
        serverResource2[0]
            .Connections.AddRange(
                [
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "192.168.201.95",
                        Port = 32400,
                        Uri = "https://192-168-201-95.fc4cf89b047845c0a48b8926677abe46.plex.direct:32400",
                        Local = true,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "www.albertflix.nl",
                        Port = 43324,
                        Uri = "https://www.albertflix.nl:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "177.170.188.28",
                        Port = 43324,
                        Uri = "https://177-170-188-28.fc4cf89b047845c0a48b8926677abe46.plex.direct:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "139.162.215.184",
                        Port = 8443,
                        Uri = "https://139-162-215-184.fc4cf89b047845c0a48b8926677abe46.plex.direct:8443",
                        Local = false,
                        Relay = true,
                        IPv6 = false,
                    },
                ]
            );
        serverResource2[1].Provides = "client,player";
        serverResource2[1].Connections.Clear();
        serverResource2[1]
            .Connections.AddRange(
                [
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "192.168.200.95",
                        Port = 32400,
                        Uri = "https://192-168-200-95.fc4cf89b047845c0a48b8926677abe46.plex.direct:32400",
                        Local = true,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "www.albertflix.nl",
                        Port = 43324,
                        Uri = "https://www.albertflix.nl:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                    new Connections
                    {
                        Protocol = Protocol.Https,
                        Address = "177.170.188.28",
                        Port = 43324,
                        Uri = "https://177-170-188-28.fc4cf89b047845c0a48b8926677abe46.plex.direct:43324",
                        Local = false,
                        Relay = false,
                        IPv6 = false,
                    },
                ]
            );
        mock.Mock<IPlexApiClient>()
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                    !request.RequestUri!.Query.Contains("includeHttps=1")
                        ? FakePlexApiData.GetHttpResponseMessage(HttpStatusCode.OK, serverResource1, request)
                        : FakePlexApiData.GetHttpResponseMessage(HttpStatusCode.OK, serverResource2, request)
            );

        // Act
        var result = await _sut.GetAccessibleServers("FAKE_TOKEN");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);

        var server1 = result.Value.First();
        server1.ShouldNotBeNull();
        server1.Connections.Count.ShouldBe(6);
    }
}
