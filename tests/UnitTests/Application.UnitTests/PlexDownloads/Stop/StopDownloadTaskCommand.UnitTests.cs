using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class StopDownloadTaskCommand_UnitTests : BaseUnitTest<StopDownloadTaskCommandHandler>
{
    public StopDownloadTaskCommand_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase();

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
        var movieDownloadTasks = await IDbContext.DownloadTaskMovie.ToListAsync();

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error"));
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>()))
            .Returns(Result.Ok());
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(
            new StopDownloadTaskCommand(movieDownloadTasks.First().Id),
            CancellationToken.None
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldNotBe(true);
    }

    [Fact]
    public async Task ShouldHaveSetDownloadTasksToStopped_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.Seed = 9999;
            config.MovieDownloadTasksCount = 2;
        });
        var movieDownloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync();

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>()))
            .Returns(Result.Ok());
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(
            new StopDownloadTaskCommand(movieDownloadTasks.First().Id),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Once);

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(movieDownloadTasks.First().ToKey());
        foreach (var downloadTaskDb in downloadTasks)
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }

    [Fact]
    public async Task ShouldHaveSetTvShowDownloadTasksToStop_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.Seed = 9999;
            config.TvShowDownloadTasksCount = 2;
            config.TvShowSeasonDownloadTasksCount = 2;
            config.TvShowEpisodeDownloadTasksCount = 2;
        });
        var tvShowDownloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync();
        var testDownloadTask = tvShowDownloadTasks.First();
        var downloadableTasks = await IDbContext.GetDownloadableChildTaskKeys(testDownloadTask.ToKey());

        downloadableTasks.Count.ShouldBe(4);

        mock.Mock<IDownloadTaskScheduler>()
            .SetupSequence(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(false)
            .ReturnsAsync(false);
        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        mock.Mock<IDirectorySystem>()
            .Setup(x => x.DeleteAllFilesFromDirectory(It.IsAny<string>()))
            .Returns(Result.Ok());
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(
            new StopDownloadTaskCommand(tvShowDownloadTasks.First().Id),
            CancellationToken.None
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Exactly(4));

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(tvShowDownloadTasks.First().ToKey());
        foreach (var downloadTaskDb in downloadTasks)
            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
    }
}
