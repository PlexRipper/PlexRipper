using System.Net.Http;
using System.Threading.Tasks;
using Logging;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace BaseTests.UnitTests.MockServer
{
    public class PlexMockServer_Setup_UnitTests
    {
        public PlexMockServer_Setup_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task ShouldDownloadFileFromMemory_WhenGivenByteArrayFileInMemory()
        {
            // Arrange
            var plexMockServer = new PlexMockServer(config =>
            {
                config.DownloadFileSizeInMb = 40;
            });
            var _httpClient = new HttpClient();

            // Act
            using var response = await _httpClient.SendAsync(new HttpRequestMessage
            {
                RequestUri = plexMockServer.DownloadUri,
                Method = HttpMethod.Get,
            }, HttpCompletionOption.ResponseHeadersRead);
            var stream = await response.Content.ReadAsByteArrayAsync();

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            stream.ShouldNotBeNull();
            stream.LongLength.ShouldBeGreaterThan(plexMockServer.DownloadFileSizeInBytes * 1000);
        }
    }
}