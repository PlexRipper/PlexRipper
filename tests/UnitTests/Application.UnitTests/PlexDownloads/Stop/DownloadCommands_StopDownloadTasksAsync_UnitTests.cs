using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests.Stop;

public class DownloadCommands_StopDownloadTasksAsync_UnitTests : BaseUnitTest<StopDownloadTaskCommandHandler>
{
    public DownloadCommands_StopDownloadTasksAsync_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase(config => config.DisableForeignKeyCheck = true);

        // Act
        var result = await _sut.Handle(new StopDownloadTaskCommand(Guid.Empty), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenTheDownloadTaskCouldNotBeStopped()
    {
        // Arrange
        await SetupDatabase(config => config.MovieDownloadTasksCount = 2);
        var movieDownloadTasks = await GetDbContext().DownloadTaskMovie.ToListAsync();

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error"));
        mock.Mock<IDirectorySystem>().Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new StopDownloadTaskCommand(movieDownloadTasks.First().Id), CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldNotBe(true);
    }

    [Fact]
    public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        Seed = 9999;
        await SetupDatabase(config => { config.MovieDownloadTasksCount = 2; });
        var movieDownloadTasks = await DbContext.GetAllDownloadTasksAsync();

        mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>())).ReturnOk();
        mock.Mock<IDirectorySystem>().Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>())).Returns(Result.Ok());
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new StopDownloadTaskCommand(movieDownloadTasks.First().Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Once);
        var downloadTasksDb = await GetDbContext()
            .DownloadTaskMovie
            .FirstOrDefaultAsync(x => x.Id == movieDownloadTasks.First().Id);
        downloadTasksDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }
}