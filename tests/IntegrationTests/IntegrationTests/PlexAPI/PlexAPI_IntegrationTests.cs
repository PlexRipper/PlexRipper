using PlexRipper.PlexApi;

namespace IntegrationTests.PlexAPI;

public class PlexApiIntegrationTests : BaseIntegrationTests
{
    public PlexApiIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveTheInterceptedClientInjected_WhenPlexAPIIsRunningInTestingMode()
    {
        // Act
        using var container = await CreateContainer(
            2363,
            config =>
            {
                config.PlexMockApiOptions = x =>
                {
                    x.MockServers.Add(new PlexMockServerConfig());
                };
            }
        );

        // Act
        var client = container.Resolve<HttpClient>();
        var wrapper = container.Resolve<PlexApiWrapper>();
        var response = await wrapper.GetAccessibleServers("asdasdas");

        // Assert
        response.ShouldNotBeNull();
        var server = response.Value.FirstOrDefault();
        server?.Connections.Count.ShouldBe(1);
        client.DefaultRequestHeaders.Contains("MockPlexApi").ShouldBeTrue();
    }
}
