using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using MediatR;
using Moq;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.UnitTests
{
    public class DownloadCommands_StopDownloadTasksAsync_UnitTests
    {


        public DownloadCommands_StopDownloadTasksAsync_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);

            // _downloadQueue.SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
            // _downloadQueue.SetupGet(x => x.UpdateDownloadTasks).Returns(new Subject<List<DownloadTask>>());
            // _fileMerger.SetupGet(x => x.FileMergeProgressObservable).Returns(new Subject<FileMergeProgress>());
            // _fileSystem.Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());
        }

        private AutoMock GetDefaultMocks()
        {
            var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
            return mock;
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenGivenAnEmptyList()
        {
            // Arrange
            using var mock = GetDefaultMocks();
            var _sut = mock.Create<DownloadCommands>();

            var list = new List<int>();

            // Act
            var result = await _sut.StopDownloadTasksAsync(list);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenGetAllRelatedDownloadTaskIdsFails()
        {
            // Arrange
            using var mock = GetDefaultMocks();
            var _sut = mock.Create<DownloadCommands>();
            mock.SetupMediator(It.IsAny<GetAllRelatedDownloadTaskIdsQuery>).ReturnsAsync(Result.Fail(""));
            var list = new List<int> { 1 };

            // Act
            var result = await _sut.StopDownloadTasksAsync(list);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
        {
            // Arrange
            using var mock = GetDefaultMocks();
            var _sut = mock.Create<DownloadCommands>();

            var downloadTasks = FakeData.GetMovieDownloadTask().Generate(1).Flatten(x => x.Children).ToList();
            var downloadTaskIds = downloadTasks.Select(x => x.Id).ToList();

            mock.SetupMediator(It.IsAny<GetAllRelatedDownloadTaskIdsQuery>).ReturnsAsync(Result.Ok(downloadTaskIds));
            mock.SetupMediator(It.IsAny<UpdateDownloadStatusOfDownloadTaskCommand>).ReturnsAsync(Result.Ok()).Verifiable();
            mock.SetupMediator(It.IsAny<GetDownloadTaskByIdQuery>).ReturnsAsync((GetDownloadTaskByIdQuery x, CancellationToken token) => Result.Ok(downloadTasks.FirstOrDefault(y => y.Id == x.Id)));

            mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result>())).ReturnsAsync(Result.Ok());

            // Act
            var result = await _sut.StopDownloadTasksAsync(downloadTaskIds);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeSubsetOf(downloadTaskIds);

            mock.Mock<IMediator>().Verify(
                x => x.Send(
                    It.Is<UpdateDownloadStatusOfDownloadTaskCommand>(y =>
                        y.DownloadTaskIds.Contains(2) && y.DownloadStatus == DownloadStatus.Stopped),
                    CancellationToken.None), Times.Once);
        }
    }
}