using Microsoft.EntityFrameworkCore;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileJobQueueUnitTests : BaseUnitTest<MoveDownloadFileJobQueue>
{
    public MoveDownloadFileJobQueueUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenNoDownloadTaskIsReadyToMove()
    {
        // Arrange — tasks exist but none are in DownloadFinished or MoveError state
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
        downloadTasks.SetDownloadStatus(DownloadStatus.Completed);
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
    public async Task ShouldRunAFileMoveJob_WhenMovieFileWithMoveErrorExists()
    {
        // Arrange — a previous move attempt failed; the queue should retry it automatically
        await SetupDatabase(
            7743,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 2;
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
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRunAFileMoveJob_WhenTvShowEpisodeFileWithMoveErrorExists()
    {
        // Arrange — a previous move attempt failed for a TV episode; the queue should retry it automatically
        await SetupDatabase(
            8812,
            config =>
            {
                config.PlexServerCount = 1;
                config.TvShowDownloadTasksCount = 2;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext.DownloadTaskTvShowEpisodeFile.AsTracking().ToListAsync(CancellationToken);
        downloadTasks.SetDownloadStatus(DownloadStatus.MoveError);
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
    public async Task ShouldRunAFileMoveJob_WhenBothMoveErrorAndDownloadFinishedTasksExist()
    {
        // Arrange — one task is in MoveError, another in DownloadFinished; the queue should pick one to process
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

        // Assert: one job is started (either the MoveError retry or the DownloadFinished task)
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
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
