using PlexRipper.PlexApi;
using PlexRipper.PlexApi.Api;

namespace IntegrationTests.BaseTests.Setup;

public class SetupMockPlexApi_IntegrationTests : BaseIntegrationTests
{
    public SetupMockPlexApi_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSpinUpOneMockPlexServerWithPlexApi_WhenGivenDefaultSettings()
    {
        // Arrange
        SpinUpPlexServer();
        SetupMockPlexApi(config => config.AccessiblePlexServers = 1);

        // Act
        var response = await GetAsync<List<ServerResource>>(PlexApiPaths.ServerResourcesUrl);

        // Assert
        MockPlexServerCount.ShouldBe(1);
        AllMockPlexServersStarted.ShouldBeTrue();
        response.Count.ShouldBe(1);
        response[0].Connections[0].Uri.ShouldBe(GetPlexServerUris[0].ToString());
    }

    [Fact]
    public async Task ShouldSpinUpFiveMockPlexServersWithPlexApi_WhenGivenDefaultSettings()
    {
        // Arrange
        SpinUpPlexServers(list =>
        {
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
        });
        SetupMockPlexApi(config => config.AccessiblePlexServers = 5);

        // Act
        var response = await GetAsync<List<ServerResource>>(PlexApiPaths.ServerResourcesUrl);

        // Assert
        MockPlexServerCount.ShouldBe(5);
        AllMockPlexServersStarted.ShouldBeTrue();
        response.Count.ShouldBe(5);

        for (var i = 0; i < 5; i++)
            response[i].Connections[0].Uri.ShouldBe(GetPlexServerUris[i].ToString());
    }

    [Fact]
    public async Task ShouldGetServerIdentityFromFiveMockedPlexServersWithPlexApi_WhenGivenDefaultSettings()
    {
        // Arrange
        SpinUpPlexServers(list =>
        {
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
        });
        SetupMockPlexApi(config => config.AccessiblePlexServers = 5);

        // Act
        var response = await GetAsync<List<ServerResource>>(PlexApiPaths.ServerResourcesUrl);
        var identityResponses = new List<ServerIdentityResponse>();
        foreach (var resource in response)
        {
            var identityResponse = await GetAsync<ServerIdentityResponse>(
                PlexApiPaths.ServerIdentity(resource.Connections[0].Uri)
            );
            identityResponses.Add(identityResponse);
        }

        // Assert
        MockPlexServerCount.ShouldBe(5);
        AllMockPlexServersStarted.ShouldBeTrue();
        response.Count.ShouldBe(5);
        identityResponses.Count.ShouldBe(5);
    }
}
