using System.Reactive.Subjects;
using Autofac.Extras.Moq;
using FluentResults;
using Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using PlexRipper.Application;
using PlexRipper.BaseTests;
using PlexRipper.BaseTests.Extensions;
using PlexRipper.Data;
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
        public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());

            // Act
            var _sut = mock.Create<DownloadCommands>();
            var result = await _sut.StopDownloadTasksAsync(0);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveFailedResult_WhenGetAllRelatedDownloadTaskIdsFails()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
            mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result>())).ReturnsAsync(Result.Ok());
            mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result<DownloadTask>>())).ReturnsAsync(Result.Ok());

            var _sut = mock.Create<DownloadCommands>();
            mock.SetupMediator(It.IsAny<GetDownloadTaskByIdQuery>).ReturnsAsync(Result.Fail(""));

            // Act
            var result = await _sut.StopDownloadTasksAsync(1);

            // Assert
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());

            await using PlexRipperDbContext context = await MockDatabase.GetMemoryDbContext().Setup(config =>
            {
                config.Seed = 9999;
                config.MovieDownloadTasksCount = 2;
            });
            var downloadTasks = await context.DownloadTasks.ToListAsync();
            downloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
            var downloadTaskIds = downloadTasks.Select(x => x.Id).ToList();

            mock.SetupMediator(It.IsAny<GetDownloadTaskByIdQuery>).ReturnsAsync(Result.Ok(downloadTasks.First()));
            mock.SetupMediator(It.IsAny<UpdateDownloadStatusOfDownloadTaskCommand>).ReturnsAsync(Result.Ok());
            mock.SetupMediator(It.IsAny<GetDownloadTaskByIdQuery>).ReturnsAsync((GetDownloadTaskByIdQuery x, CancellationToken _) =>
                Result.Ok(downloadTasks.FirstOrDefault(y => y.Id == x.Id)));

            mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result>())).ReturnsAsync(Result.Ok());
            mock.Mock<IDownloadTracker>().Setup(x => x.StopDownloadClient(It.IsAny<int>())).ReturnsAsync(Result.Ok());
            mock.Mock<IDirectorySystem>().Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());

            // Act
            var _sut = mock.Create<DownloadCommands>();
            var result = await _sut.StopDownloadTasksAsync(downloadTaskIds.First());

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