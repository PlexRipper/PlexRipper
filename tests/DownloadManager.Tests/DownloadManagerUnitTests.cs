using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.Tests
{
    public class DownloadManagerUnitTests
    {
        private BaseContainer Container { get; }

        public DownloadManagerUnitTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task TestDownloadClient()
        {
            // Arrange
            var downloadManager = Container.GetDownloadManager;
            var downloadTask = new DownloadTask
            {
                PlexServer = new PlexServer
                {
                    Scheme = "http",
                    Address = "80.112.5.167",
                    Port = 43234,
                },
            };

            //  // Act
            // var result = await downloadManager.CreateDownload(downloadTask);
            //
            //  // Assert
            //  result.IsFailed.ShouldBeFalse();
            //  result.Value.ShouldBeTrue();
        }
    }
}