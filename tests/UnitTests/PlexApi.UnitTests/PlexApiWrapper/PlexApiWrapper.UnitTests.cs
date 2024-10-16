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
    public async Task Should_When()
    {
        // Arrange
        var response1 = FakePlexApiData
            .GetServerResourcesResponse(
                HttpStatusCode.OK,
                config =>
                {
                    config.Seed = 939;
                }
            )
            .Generate();
        var response2 = FakePlexApiData
            .GetServerResourcesResponse(
                HttpStatusCode.OK,
                config =>
                {
                    config.Seed = 939;
                    config.PlexServerAccessConnectionsIncludeHttps = true;
                }
            )
            .Generate();

        mock.Mock<IPlexApiClient>()
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                    FakePlexApiData.GetHttpResponseMessage(
                        HttpStatusCode.OK,
                        request.RequestUri!.Query.Contains("includeHttps=1")
                            ? response2.PlexDevices
                            : response1.PlexDevices,
                        request
                    )
            );

        // Act
        var result = await _sut.GetAccessibleServers("FAKE_TOKEN");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }
}
