using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.DownloadManager;

namespace DownloadManager.UnitTests;

public class DownloadCommands_StopDownloadTasksAsync_UnitTests : BaseUnitTest<DownloadCommands>
{
    public DownloadCommands_StopDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());

        // Act
        var result = await _sut.StopDownloadTasksAsync(0);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGetAllRelatedDownloadTaskIdsFails()
    {
        // Arrange
        mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
        mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result>())).ReturnsAsync(Result.Ok());
        mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result<DownloadTask>>())).ReturnsAsync(Result.Ok());

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
        Seed = 9999;
        mock.Mock<IDownloadQueue>().SetupGet(x => x.StartDownloadTask).Returns(new Subject<DownloadTask>());
        await SetupDatabase(config =>
        {
            config.PlexServerCount = 1;
            config.PlexLibraryCount = 1;
            config.MovieCount = 5;
            config.MovieDownloadTasksCount = 2;
        });

        var downloadTasks = await DbContext.DownloadTasks.ToListAsync();
        downloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        var downloadTaskIds = downloadTasks.Select(x => x.Id).ToList();

        mock.SetupMediator(It.IsAny<GetDownloadTaskByIdQuery>).ReturnsAsync(Result.Ok(downloadTasks.First()));
        mock.SetupMediator(It.IsAny<UpdateDownloadStatusOfDownloadTaskCommand>).ReturnsAsync(Result.Ok());
        mock.SetupMediator(It.IsAny<GetDownloadTaskByIdQuery>)
            .ReturnsAsync((GetDownloadTaskByIdQuery x, CancellationToken _) =>
                Result.Ok(downloadTasks.FirstOrDefault(y => y.Id == x.Id)));

        mock.Mock<INotificationsService>().Setup(x => x.SendResult(It.IsAny<Result>())).ReturnsAsync(Result.Ok());
        mock.Mock<IDownloadTracker>().Setup(x => x.StopDownloadClient(It.IsAny<int>())).ReturnsAsync(Result.Ok());
        mock.Mock<IDirectorySystem>().Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());

        // Act
        var result = await _sut.StopDownloadTasksAsync(downloadTaskIds.First());

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSubsetOf(downloadTaskIds);

        mock.Mock<IMediator>()
            .Verify(
                x => x.Send(
                    It.IsAny<UpdateDownloadStatusOfDownloadTaskCommand>(),
                    It.IsAny<CancellationToken>()), Times.Once);
    }
}