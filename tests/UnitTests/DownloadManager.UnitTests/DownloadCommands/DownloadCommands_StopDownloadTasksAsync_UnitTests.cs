using BackgroundServices.Contracts;
using DownloadManager.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Application;
using PlexRipper.Data.Common;

namespace DownloadManager.UnitTests;

public class DownloadCommands_StopDownloadTasksAsync_UnitTests : BaseUnitTest<StopDownloadTaskCommandHandler>
{
    public DownloadCommands_StopDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);

        // Act
        var result = await _sut.Handle(new StopDownloadTaskCommand(0), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenTheDownloadTaskCouldNotBeStopped()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.MovieDownloadTasksCount = 2;
            config.DisableForeignKeyCheck = true;
        });
        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StopDownloadTaskJob(It.IsAny<int>())).ReturnsAsync(Result.Fail("Error"));
        mock.Mock<IDirectorySystem>().Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());
        mock.SetupMediator(It.IsAny<DownloadTaskUpdated>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new StopDownloadTaskCommand(1), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        Seed = 9999;
        await SetupDatabase(config =>
        {
            config.MovieDownloadTasksCount = 2;
            config.DisableForeignKeyCheck = true;
        });

        var downloadTasks = await DbContext.DownloadTasks.Include(downloadTask => downloadTask.Children).ToListAsync();
        downloadTasks = downloadTasks.Flatten(x => x.Children).ToList();
        var downloadTaskIds = downloadTasks.Select(x => x.Id).ToList();

        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StopDownloadTaskJob(It.IsAny<int>())).ReturnOk();
        mock.Mock<IDirectorySystem>().Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());
        mock.SetupMediator(It.IsAny<DownloadTaskUpdated>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new StopDownloadTaskCommand(downloadTaskIds.First()), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<int>()), Times.Once);
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdated>, Times.Once);
        var downloadTasksDb = await GetDbContext().DownloadTasks
            .IncludeDownloadTasks()
            .FirstOrDefaultAsync(x => x.Id == downloadTaskIds.First());
        downloadTasksDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }
}