using Logging;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Domain.UnitTests.Converters
{
    public class DownloadTaskActions_Aggregate_UnitTests
    {
        public DownloadTaskActions_Aggregate_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public void ShouldBeStatusDownloading_WhenSomeAreDownloadFinished()
        {
            // Arrange
            var downloadStatusList = new List<DownloadStatus>
            {
                DownloadStatus.Downloading,
                DownloadStatus.Downloading,
                DownloadStatus.DownloadFinished,
                DownloadStatus.DownloadFinished,
            };

            // Act
            var status = DownloadTaskActions.Aggregate(downloadStatusList);

            // Assert
            status.ShouldBe(DownloadStatus.Downloading);
        }

        [Fact]
        public void ShouldBeStatusDownloadingFinished_WhenAllAreDownloadFinished()
        {
            // Arrange
            var downloadStatusList = new List<DownloadStatus>
            {
                DownloadStatus.DownloadFinished,
                DownloadStatus.DownloadFinished,
                DownloadStatus.DownloadFinished,
                DownloadStatus.DownloadFinished,
            };

            // Act
            var status = DownloadTaskActions.Aggregate(downloadStatusList);

            // Assert
            status.ShouldBe(DownloadStatus.DownloadFinished);
        }

        [Fact]
        public void ShouldBeStatusDownloading_WhenSomeAreDownloadFinishedAndQueued()
        {
            // Arrange
            var downloadStatusList = new List<DownloadStatus>
            {
                DownloadStatus.DownloadFinished,
                DownloadStatus.DownloadFinished,
                DownloadStatus.Queued,
                DownloadStatus.Queued,
            };

            // Act
            var status = DownloadTaskActions.Aggregate(downloadStatusList);

            // Assert
            status.ShouldBe(DownloadStatus.Downloading);
        }

        [Fact]
        public void ShouldBeStatusDownloadingFinished_WhenSomeAreCompletedAndDownloadFinished()
        {
            // Arrange
            var downloadStatusList = new List<DownloadStatus>
            {
                DownloadStatus.DownloadFinished,
                DownloadStatus.DownloadFinished,
                DownloadStatus.Completed,
                DownloadStatus.Completed,
            };

            // Act
            var status = DownloadTaskActions.Aggregate(downloadStatusList);

            // Assert
            status.ShouldBe(DownloadStatus.DownloadFinished);
        }
    }
}