using Humanizer.Bytes;

namespace IntegrationTests.BaseTests.Setup;

public class SpinUpPlexServerIntegrationTests : BaseIntegrationTests
{
    public SpinUpPlexServerIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSpinUpFiveMockPlexServers_WhenGivenDefaultSettings()
    {
        // Arrange
        using var container = await CreateContainer(
            213651,
            config =>
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
            }
        );

        // Assert
        container.PlexMockServers.Count.ShouldBe(5);
        container.PlexMockServers.Select(x => x.IsStarted).ToList().ShouldAllBe(x => x == true);
    }

    [Fact]
    public async Task ShouldDownloadFileFromMemory_WhenGivenByteArrayFileInMemory()
    {
        // Arrange
        var mbSize = 50;
        using var container = await CreateContainer(
            235253,
            config =>
            {
                config.PlexMockApiOptions = x =>
                {
                    x.MockServers.AddRange([new PlexMockServerConfig() { DownloadFileSizeInMb = mbSize }]);
                };
            }
        );

        container.PlexMockServers.Count.ShouldBe(1);

        var uri = container.PlexMockServers.First().ServerUri;
        var urlBuilder = new UriBuilder(uri)
        {
            Path = PlexMockServerConfig.FileUrl,
            Query = "X-Plex-Token=EHRWERHAERHAERH",
        };

        var request = new HttpRequestMessage { RequestUri = urlBuilder.Uri, Method = HttpMethod.Get };

        // Act
        using var client = container.Resolve<HttpClient>();
        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var stream = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        stream.ShouldNotBeNull();
        stream.LongLength.ShouldBeEquivalentTo((long)ByteSize.FromMegabytes(mbSize).Bytes);
    }
}
