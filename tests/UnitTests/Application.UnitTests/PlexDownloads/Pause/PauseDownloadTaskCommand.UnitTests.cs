using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadCommandsPauseDownloadTasksAsyncUnitTests : BaseUnitTest<PauseDownloadTaskCommandHandler>
{
    public DownloadCommandsPauseDownloadTasksAsyncUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenGivenAnInvalidId()
    {
        // Arrange
        await SetupDatabase(34006);

        // Act
        var result = await Sut.ExecuteAsync(new PauseDownloadTaskCommand(Guid.Empty), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHaveFailedResult_WhenTheDownloadTaskCouldNotBeStopped()
    {
        // Arrange
        await SetupDatabase(30082, config => config.MovieDownloadTasksCount = 2);
        var movieDownloadTasks = await IDbContext.DownloadTaskMovie.ToListAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Error"));
        Mock.SetupCommand(It.IsAny<DownloadTaskUpdatedCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(
            new PauseDownloadTaskCommand(movieDownloadTasks.First().Id),
            CancellationToken
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Has404NotFoundError().ShouldNotBe(true);
    }

    [Fact]
    public async Task ShouldHaveSetMovieDownloadTasksToPaused_WhenAtLeastOneValidIdIsGiven()
    {
        // Arrange
        await SetupDatabase(9999, config => config.MovieDownloadTasksCount = 2);
        var movieDownloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        var testDownloadTask = movieDownloadTasks.First().ToKey();

        var downloadableTasks = await IDbContext.GetDownloadableChildTaskKeys(testDownloadTask, CancellationToken);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == downloadableTasks.First().Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk()
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.ExecuteAsync(new PauseDownloadTaskCommand(testDownloadTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCallStopDownloadJob_WhenTaskIsDownloadingAndAtLeastOneValidIdIsGiven()
    {
        // Arrange
        await SetupDatabase(
            19965,
            config =>
            {
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 2;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );
        var tvShowDownloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            cancellationToken: CancellationToken
        );
        var testDownloadTask = tvShowDownloadTasks.First().ToKey();
        var downloadableTasks = await IDbContext.GetDownloadableChildTaskKeys(testDownloadTask, CancellationToken);

        downloadableTasks.Count.ShouldBe(4);

        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == downloadableTasks.First().Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<IDownloadTaskScheduler>()
            .SetupSequence(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(false)
            .ReturnsAsync(false);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnOk()
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.ExecuteAsync(new PauseDownloadTaskCommand(testDownloadTask.Id), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}
