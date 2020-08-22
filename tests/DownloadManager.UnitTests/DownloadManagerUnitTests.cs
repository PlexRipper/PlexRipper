using System.Threading.Tasks;
using PlexRipper.BaseTests;
using PlexRipper.Domain.Entities;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadManagerUnitTests
    {
        private BaseContainer Container { get; }

        public DownloadManagerUnitTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.Setup(output);
            Container = new BaseContainer();
        }

        [Fact]
        public async Task TestDownloadClient()
        {
            // Arrange
            var downloadManager = Container.GetDownloadManager;
            var downloadTask = new DownloadTask
            {
                FileLocationUrl = "/library/parts/41722/1519652720/file.mkv",
                PlexServer = new PlexServer
                {
                    Scheme = "http",
                    Address = "80.112.5.167",
                    Port = 43234,

                }
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