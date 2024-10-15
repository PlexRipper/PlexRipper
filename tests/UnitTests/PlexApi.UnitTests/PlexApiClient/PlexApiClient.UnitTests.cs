using System.Net;
using Moq.Contrib.HttpClient;
using PlexApi.Contracts;
using PlexRipper.PlexApi;

namespace PlexApi.UnitTests;

public class PlexApiClientUnitTests : BaseUnitTest<Func<PlexApiClientOptions?, PlexApiClient>>
{
    public PlexApiClientUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task Should_When()
    {
        SetupHttpClient(config =>
        {
            config.SetupAnyRequest().ReturnsResponse(HttpStatusCode.NotFound);
        });

        // Arrange
        var client = _sut(new PlexApiClientOptions { ConnectionUrl = "http://localhost", Action = null });

        // Act
        var result = await client.SendAsync(new HttpRequestMessage());

        // Assert
        result.ShouldNotBeNull();

        await Task.CompletedTask;
    }
}
