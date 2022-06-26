using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadQueue_AddToDownloadQueue_UnitTests
    {
        public DownloadQueue_AddToDownloadQueue_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Fact]
        public async Task AddToDownloadQueueAsync_ShouldReturnFailedResult_WhenEmptyListIsGiven()
        {
            //Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<DownloadQueue>();

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
            mock.Mock<IDownloadTaskValidator>().Setup(x => x.ValidateDownloadTasks(It.IsAny<List<DownloadTask>>())).Returns(Result.Fail(""));

            // Act
            var _sut = mock.Create<DownloadQueue>();
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