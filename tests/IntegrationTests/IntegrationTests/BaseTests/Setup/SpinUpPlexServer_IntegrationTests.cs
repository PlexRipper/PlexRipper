using Humanizer.Bytes;

namespace IntegrationTests.BaseTests.Setup;

[Collection("Sequential")]
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

    [Fact]
    public async Task ShouldDownloadFileFromMemory_WhenGivenByteArrayFileInMemory()
    {
        // Arrange
        var mbSize = 50;
        var uri = SpinUpPlexServer(config => config.DownloadFileSizeInMb = mbSize);
        var urlBuilder = new UriBuilder(uri)
        {
            Path = PlexMockServerConfig.FileUrl,
            Query = "X-Plex-Token=EHRWERHAERHAERH",
        };

        var request = new HttpRequestMessage
        {
            RequestUri = urlBuilder.Uri,
            Method = HttpMethod.Get,
        };

        // Act
        using var response = await SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        var stream = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        stream.ShouldNotBeNull();
        stream.LongLength.ShouldBeEquivalentTo((long)ByteSize.FromMegabytes(mbSize).Bytes);
    }
}