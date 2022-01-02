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
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenGivenAnEmptyList()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());

            var list = new List<int>();

            // Act
            var _sut = mock.Create<DownloadCommands>();
            var result = await _sut.StopDownloadTasksAsync(list);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenGetAllRelatedDownloadTaskIdsFails()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());

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
            using var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());

            var downloadTasks = FakeData.GetMovieDownloadTask().Generate(1).Flatten(x => x.Children).ToList();
            var downloadTaskIds = downloadTasks.Select(x => x.Id).ToList();

            mock.SetupMediator(It.IsAny<GetAllRelatedDownloadTaskIdsQuery>).ReturnsAsync(Result.Ok(downloadTaskIds));
            mock.SetupMediator(It.IsAny<UpdateDownloadStatusOfDownloadTaskCommand>).ReturnsAsync(Result.Ok());
            mock.SetupMediator(It.IsAny<GetDownloadTaskByIdQuery>).ReturnsAsync((GetDownloadTaskByIdQuery x, CancellationToken token) =>
                Result.Ok(downloadTasks.FirstOrDefault(y => y.Id == x.Id)));

            mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result>())).ReturnsAsync(Result.Ok());
            mock.Mock<IDownloadTracker>().Setup(x => x.StopDownloadClient(It.IsAny<int>())).ReturnsAsync(Result.Ok());
            mock.Mock<IDirectorySystem>().Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());

            // Act
            var _sut = mock.Create<DownloadCommands>();
            var result = await _sut.StopDownloadTasksAsync(downloadTaskIds);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeSubsetOf(downloadTaskIds);

            mock.Mock<IMediator>().Verify(
                x => x.Send(
                    It.IsAny<UpdateDownloadStatusOfDownloadTaskCommand>(),
                     It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}