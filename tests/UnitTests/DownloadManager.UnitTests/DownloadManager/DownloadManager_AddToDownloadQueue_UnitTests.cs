using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadManager_AddToDownloadQueue_UnitTests
    {

        public DownloadManager_AddToDownloadQueue_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenEmptyListIsGiven()
        {
            //Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<PlexRipper.DownloadManager.DownloadManager>();

            // Act
            var result = await _sut.AddToDownloadQueueAsync(new List<DownloadTask>());

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenInvalidDownloadTasksAreGiven()
        {
            //Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<PlexRipper.DownloadManager.DownloadManager>();

            // Act
            var result = await _sut.AddToDownloadQueueAsync(new List<DownloadTask>
            {
                new(),
                new(),
            });

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

    }
}