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
            var config = new PlexMockServerConfig()
            {
                File = FakeData.GetDownloadFile(40),
            };
            var plexMockServer = new PlexMockServer(config);
            var _httpClient = new HttpClient();

            // Act
            using var response = await _httpClient.SendAsync(new HttpRequestMessage
            {
                RequestUri = plexMockServer.GetDownloadUri,
                Method = HttpMethod.Get,
            }, HttpCompletionOption.ResponseHeadersRead);
            var stream = await response.Content.ReadAsByteArrayAsync();

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
            stream.ShouldNotBeNull();
            stream.LongLength.ShouldBe(config.File.LongLength);
        }
    }
}