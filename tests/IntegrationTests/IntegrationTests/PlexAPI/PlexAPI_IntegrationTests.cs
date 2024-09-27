using PlexRipper.PlexApi;

namespace IntegrationTests;

public class PlexAPI_IntegrationTests : BaseIntegrationTests
{
    public PlexAPI_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveTheInterceptedClientInjected_WhenPlexAPIIsRunningInTestingMode()
    {
        // Act
        await CreateContainer(config =>
        {
            config.PlexMockApiOptions = x =>
            {
                x.MockServers.Add(new PlexMockServerConfig());
            };
        });

        // Act
        var client = Container.Resolve<HttpClient>();
        var wrapper = Container.Resolve<PlexApiWrapper>();
        var response = await wrapper.GetAccessibleServers("asdasdas");

        // Assert
        response.ShouldNotBeNull();
        var server = response.Value.FirstOrDefault();
        server?.Connections.Count.ShouldBe(1);
        client.DefaultRequestHeaders.Contains("MockPlexApi").ShouldBeTrue();
    }
}
