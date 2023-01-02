namespace BaseTests.IntegrationTests.Setup;

public class SpinUpPlexServer_IntegrationTests : BaseIntegrationTests
{
    public SpinUpPlexServer_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void ShouldSpinUpOneMockPlexServer_WhenGivenDefaultSettings()
    {
        // Act
        SpinUpPlexServer();

        // Assert
        MockPlexServerCount.ShouldBe(1);
        AllMockPlexServersStarted.ShouldBeTrue();
    }

    [Fact]
    public void ShouldSpinUpFiveMockPlexServers_WhenGivenDefaultSettings()
    {
        // Act
        SpinUpPlexServers(list =>
        {
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
            list.Add(new PlexMockServerConfig());
        });

        // Assert
        MockPlexServerCount.ShouldBe(5);
        AllMockPlexServersStarted.ShouldBeTrue();
    }
}