using System.Net;
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
            config =>
            {
                config.PlexServerAccessCount = serverCount;
            }
        );

        var response2 = FakePlexApiData.GetServerResourcesResponse(
            HttpStatusCode.OK,
            new Seed(939),
            config =>
            {
                config.PlexServerAccessCount = serverCount;
                config.PlexServerAccessConnectionsIncludeHttps = true;
            }
        );

        var testConnection = response1.PlexDevices[0].Connections[0];

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
        connections.Any(x => x.Uri == testConnection.Uri).ShouldBeFalse();
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
        var response2 = FakePlexApiData.GetServerResourcesResponse(
            HttpStatusCode.OK,
            new Seed(939),
            config =>
            {
                config.PlexServerAccessCount = serverCount;
                config.PlexServerAccessConnectionsIncludeHttps = true;
            }
        );

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
                        : FakePlexApiData.GetHttpResponseMessage(HttpStatusCode.OK, response2.PlexDevices, request)
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
        var response1 = FakePlexApiData.GetServerResourcesResponse(
            HttpStatusCode.OK,
            new Seed(939),
            config =>
            {
                config.PlexServerAccessCount = serverCount;
            }
        );

        var response2 = FakePlexApiData.GetServerResourcesResponse(
            HttpStatusCode.OK,
            new Seed(940),
            config =>
            {
                config.PlexServerAccessCount = serverCount;
                config.PlexServerAccessConnectionsIncludeHttps = true;
            }
        );

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

        var allConnections = result.Value.SelectMany(x => x.Connections).ToList();
        allConnections.ShouldNotBeEmpty();
        allConnections.Select(c => c.Uri).ShouldBeUnique();
    }
}
