using LukeHagar.PlexAPI.SDK;
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
        await CreateContainer();

        var wrapper = Container.Resolve<PlexApiWrapper>();
        var httpClient = Container.Resolve<HttpClient>();
        var clientFactory = Container.Resolve<Func<PlexApiClientOptions?, PlexApiClient>>();

        var plexApi = new PlexAPI(
            client: clientFactory(new PlexApiClientOptions() { ConnectionUrl = "https://plex.tv/api/v2/" }),
            clientID: "TESTCLIENTID",
            serverUrl: "https://plex.tv/api/v2/",
            accessToken: "TESTTOKEN"
        );

        var response = await wrapper.GetAccessibleServers("asdasdas");

        response.ShouldNotBeNull();
    }
}
