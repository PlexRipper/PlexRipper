using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class PlexDownloadClientUnitTests
    {
        private BaseContainer Container { get; }

        public PlexDownloadClientUnitTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
            Container.SetupTestAccount();
        }

        [Fact]
        public async Task TestDownloadClient()
        {
            // Arrange
            var downloadTask = new DownloadTask
            {
                PlexServer = new PlexServer
                {
                    Scheme = "http",
                    Address = "80.112.5.167",
                    Port = 43234,
                },
            };
            var downloadClient = Container.GetPlexDownloadClientFactory(downloadTask);

            await downloadClient.Start();

            //  // Act
            // var result = await downloadManager.CreateDownload(downloadTask);
            //
            //  // Assert
            //  result.IsFailed.ShouldBeFalse();
            //  result.Value.ShouldBeTrue();
        }
    }
}