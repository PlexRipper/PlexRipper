using Application.Contracts;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

public class DownloadCommands_PauseDownloadTasksAsync_UnitTests : BaseUnitTest<PauseDownloadTaskCommandHandler>
{
    public DownloadCommands_PauseDownloadTasksAsync_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase();

        // Act
        var result = await _sut.Handle(new PauseDownloadTaskCommand(Guid.Empty), CancellationToken.None);

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
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error"));
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(
            new PauseDownloadTaskCommand(movieDownloadTasks.First().Id),
            CancellationToken.None
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldNotBe(true);
    }

    [Fact]
    public async Task ShouldHaveSetMovieDownloadTasksToPaused_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.Seed = 9999;
            config.MovieDownloadTasksCount = 2;
        });
        var movieDownloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync();
        var testDownloadTask = movieDownloadTasks.First().ToKey();

        var downloadableTasks = await IDbContext.GetDownloadableChildTaskKeys(testDownloadTask);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == downloadableTasks.First().Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading));

        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new PauseDownloadTaskCommand(testDownloadTask.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Once);

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(movieDownloadTasks.First().ToKey());
        for (var index = 0; index < downloadTasks.Count; index++)
        {
            var downloadTaskDb = downloadTasks[index];
            if (index > 0)
            {
                downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Queued);
                continue;
            }

            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Paused);
        }
    }

    [Fact]
    public async Task ShouldHaveSetTvShowDownloadTasksToPaused_WhenAtLeastOneValidIdIsGiven()
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
        var testDownloadTask = tvShowDownloadTasks.First().ToKey();
        var downloadableTasks = await IDbContext.GetDownloadableChildTaskKeys(testDownloadTask);

        downloadableTasks.Count.ShouldBe(4);

        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadableTasks.First().Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading));

        mock.Mock<IDownloadTaskScheduler>()
            .SetupSequence(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(false)
            .ReturnsAsync(false);
        mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk();
        mock.SetupMediator(It.IsAny<DownloadTaskUpdatedNotification>).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new PauseDownloadTaskCommand(testDownloadTask.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        mock.VerifyMediator(It.IsAny<DownloadTaskUpdatedNotification>, Times.Once);

        var downloadTasks = await IDbContext.GetDownloadableChildTasks(tvShowDownloadTasks.First().ToKey());
        for (var index = 0; index < downloadTasks.Count; index++)
        {
            var downloadTaskDb = downloadTasks[index];
            if (index > 0)
            {
                downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Queued);
                continue;
            }

            downloadTaskDb.DownloadStatus.ShouldBe(DownloadStatus.Paused);
        }
    }
}
