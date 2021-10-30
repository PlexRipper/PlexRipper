using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using Moq;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadManager_StopDownloadTasksAsync_UnitTests
    {
        private readonly Mock<PlexRipper.DownloadManager.DownloadManager> _sut;

        private readonly Mock<IMediator> _iMediator = new();

        private readonly Mock<ISignalRService> _signalRService = new();

        private readonly Mock<IFileSystem> _fileSystem = new();

        private readonly Mock<IFileMerger> _fileMerger = new();

        private readonly Mock<IDownloadQueue> _downloadQueue = new();

        private readonly Mock<INotificationsService> _notificationsService = new();

        private readonly Mock<IPlexDownloadTaskFactory> _plexDownloadTaskFactory = new();

        private readonly Mock<IPlexRipperHttpClient> _httpClient = new();

        public DownloadManager_StopDownloadTasksAsync_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            _sut = new Mock<PlexRipper.DownloadManager.DownloadManager>(
                MockBehavior.Strict,
                _iMediator.Object,
                _signalRService.Object,
                _fileSystem.Object,
                _fileMerger.Object,
                _downloadQueue.Object,
                _notificationsService.Object,
                _plexDownloadTaskFactory.Object,
                _httpClient.Object);

            _downloadQueue.SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
            _downloadQueue.SetupGet(x => x.UpdateDownloadTasks).Returns(new Subject<List<DownloadTask>>());
            _fileMerger.SetupGet(x => x.FileMergeProgressObservable).Returns(new Subject<FileMergeProgress>());
            _fileSystem.Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenGivenAnEmptyList()
        {
            // Arrange
            var list = new List<int>();

            // Act
            var result = await _sut.Object.StopDownloadTasksAsync(list);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenGetAllRelatedDownloadTaskIdsFails()
        {
            // Arrange
            _iMediator.Setup(x => x.Send(It.IsAny<GetAllRelatedDownloadTaskIdsQuery>(), CancellationToken.None))
                .ReturnsAsync(Result.Fail(""));
            var list = new List<int> { 1 };

            // Act
            var result = await _sut.Object.StopDownloadTasksAsync(list);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
        {
            // Arrange
            var downloadTasks = FakeData.GetMovieDownloadTask().Generate(1).Flatten(x => x.Children).ToList();
            var downloadTaskIds = downloadTasks.Select(x => x.Id).ToList();
            _iMediator.Setup(x => x.Send(It.IsAny<GetAllRelatedDownloadTaskIdsQuery>(), CancellationToken.None))
                .ReturnsAsync(Result.Ok(downloadTaskIds));

            _iMediator.Setup(x => x.Send(It.IsAny<GetDownloadTaskByIdQuery>(), CancellationToken.None))
                .ReturnsAsync((GetDownloadTaskByIdQuery x, CancellationToken token) => Result.Ok(downloadTasks.FirstOrDefault(y => y.Id == x.Id)));

            _notificationsService.Setup(x => x.SendResult(It.IsAny<Result>())).ReturnsAsync(Result.Ok());

            _iMediator.Setup(x => x.Send(It.IsAny<UpdateDownloadStatusOfDownloadTaskCommand>(), CancellationToken.None)).ReturnsAsync(Result.Ok())
                .Verifiable();

            // Act
            var result = await _sut.Object.StopDownloadTasksAsync(downloadTaskIds);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeSubsetOf(downloadTaskIds);
            _iMediator.Verify(
                x => x.Send(
                    It.Is<UpdateDownloadStatusOfDownloadTaskCommand>(y =>
                        y.DownloadTaskIds.Contains(2) && y.DownloadStatus == DownloadStatus.Stopped),
                    CancellationToken.None), Times.Once);
        }
    }
}