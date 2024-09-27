using Humanizer.Bytes;

namespace IntegrationTests.BaseTests.Setup;

public class SpinUpPlexServer_IntegrationTests : BaseIntegrationTests
{
    public SpinUpPlexServer_IntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSpinUpFiveMockPlexServers_WhenGivenDefaultSettings()
    {
        // Arrange
        await CreateContainer(config =>
        {
            config.PlexMockApiOptions = x =>
            {
                x.MockServers.AddRange(
                    [
                        new PlexMockServerConfig(),
                        new PlexMockServerConfig(),
                        new PlexMockServerConfig(),
                        new PlexMockServerConfig(),
                        new PlexMockServerConfig(),
                    ]
                );
            };
        });

        // Assert
        Container.PlexMockServers.Count.ShouldBe(5);
        Container.PlexMockServers.Select(x => x.IsStarted).ToList().ShouldAllBe(x => x == true);
    }

    [Fact]
    public async Task ShouldDownloadFileFromMemory_WhenGivenByteArrayFileInMemory()
    {
        // Arrange
        var mbSize = 50;
        await CreateContainer(config =>
        {
            config.PlexMockApiOptions = x =>
            {
                x.MockServers.AddRange([new PlexMockServerConfig() { DownloadFileSizeInMb = mbSize }]);
            };
        });

        Container.PlexMockServers.Count.ShouldBe(1);

        var uri = Container.PlexMockServers.First().ServerUri;
        var urlBuilder = new UriBuilder(uri)
        {
            Path = PlexMockServerConfig.FileUrl,
            Query = "X-Plex-Token=EHRWERHAERHAERH",
        };

        var request = new HttpRequestMessage { RequestUri = urlBuilder.Uri, Method = HttpMethod.Get };

        // Act
        using var client = Container.Resolve<HttpClient>();
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var stream = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        stream.ShouldNotBeNull();
        stream.LongLength.ShouldBeEquivalentTo((long)ByteSize.FromMegabytes(mbSize).Bytes);
    }
}
