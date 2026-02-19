using Microsoft.EntityFrameworkCore;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileJobQueueUnitTests : BaseUnitTest<MoveDownloadFileJobQueue>
{
    public MoveDownloadFileJobQueueUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenNoDownloadTaskIsInDownloadFinishedState()
    {
        // Arrange — tasks exist but none are in DownloadFinished state (e.g. all in MoveError after a prior failure)
        await SetupDatabase(
            9876,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 3;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        downloadTasks.SetDownloadStatus(DownloadStatus.MoveError);
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: returns success — "nothing to move" is not an error, just a no-op
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldPickNextDownloadFinishedTask_WhenPreviousMoveErroredAndMoreTasksAreReady()
    {
        // Arrange — one task is in MoveError, another is in DownloadFinished
        // This simulates the stuck-queue scenario: a move fails, then a subsequent download completes.
        // The queue must move past the errored task and process the ready one.
        await SetupDatabase(
            5544,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 2;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        downloadTasks[0].DownloadStatus = DownloadStatus.MoveError;
        downloadTasks[1].DownloadStatus = DownloadStatus.DownloadFinished;
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: the queue skips the MoveError task and successfully starts a job for the DownloadFinished one
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(
                x => x.StartMoveDownloadFileJob(It.Is<DownloadTaskKey>(k => k.Id == downloadTasks[1].Id)),
                Times.Once
            );
    }

    [Fact]
    public async Task ShouldNotRunAnotherFileMoveJob_WhenAJobIsAlreadyRunning()
    {
        // Arrange
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: already running is not an error, just a no-op
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotRunFileMoveJob_WhenNoDownloadTaskIsAvailable()
    {
        await SetupDatabase(9, config => config.PlexServerCount = 1);

        // Arrange
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: already running is not an error, job is skipped as a no-op
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRunAFileMoveJob_WhenDownloadTaskMovieFileWithDownloadFinishedExist()
    {
        // Arrange
        await SetupDatabase(
            9999,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 5;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        downloadTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRunAFileMoveJob_WhenDownloadTaskTvShowEpisodeFileWithDownloadFinishedExist()
    {
        // Arrange
        await SetupDatabase(
            9999,
            config =>
            {
                config.PlexServerCount = 1;
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 5;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskTvShowEpisodeFile.AsTracking().ToListAsync(CancellationToken);
        downloadTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        await dbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }
}
