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
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never);
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
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
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
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
    }

    [Fact]
    public async Task ShouldRunAFileMoveJob_WhenBothMoveErrorAndDownloadFinishedTasksExist()
    {
        // Arrange — one task is in MoveError, another in DownloadFinished; DownloadFinished is preferred by the queue
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

        var expectedKey = downloadTasks[1].ToKey();

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        DownloadTaskKey? capturedKey = null;
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .Callback<DownloadTaskKey>(k => capturedKey = k)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: the DownloadFinished task is preferred over MoveError
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
        capturedKey.ShouldNotBeNull();
        capturedKey.ShouldBe(expectedKey);
    }

    [Fact]
    public async Task ShouldNotRunAnotherFileMoveJob_WhenAJobIsAlreadyRunning()
    {
        // Arrange
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: already running is not an error, just a no-op; no new job must be started
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never);
    }

    [Fact]
    public async Task ShouldNotRunFileMoveJob_WhenNoDownloadTaskIsAvailable()
    {
        // Arrange
        await SetupDatabase(9, config => config.PlexServerCount = 1);

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

        // Assert: no task available is not an error, job is never started
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Never);
    }

    [Fact]
    public async Task ShouldReturnFailureResult_WhenStartMoveDownloadFileJobFails()
    {
        // Arrange
        await SetupDatabase(
            6661,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 1;
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

        var startResult = Result.Fail("Scheduler failed to start job");
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(startResult)
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: the scheduler failure is propagated back to the caller
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldBe(startResult.Errors);
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
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
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
    }

    [Fact]
    public async Task ShouldPreferMovieFileOverTvEpisodeFile_WhenBothAreEligibleToMove()
    {
        // Arrange — both a movie and a TV episode are DownloadFinished; the SUT must pick the movie
        await SetupDatabase(
            1122,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFileTasks = await dbContext.DownloadTaskMovieFile.AsTracking().ToListAsync(CancellationToken);
        movieFileTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        var episodeFileTasks = await dbContext
            .DownloadTaskTvShowEpisodeFile.AsTracking()
            .ToListAsync(CancellationToken);
        episodeFileTasks.SetDownloadStatus(DownloadStatus.DownloadFinished);
        await dbContext.SaveChangesAsync(CancellationToken);

        var expectedKey = movieFileTasks.First().ToKey();

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        DownloadTaskKey? capturedKey = null;
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .Callback<DownloadTaskKey>(k => capturedKey = k)
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await Sut.CheckMoveDownloadFileJobQueue();

        // Assert: the movie file is preferred over the TV episode file
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
        capturedKey.ShouldNotBeNull();
        capturedKey.ShouldBe(expectedKey);
    }

    [Fact]
    public async Task ShouldRunAFileMoveJob_WhenDownloadTaskTvShowEpisodeFileWithDownloadFinishedExist()
    {
        // Arrange
        await SetupDatabase(
            10000,
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
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.IsAnyMoveDownloadFileJobRunning(), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>()
            .Verify(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()), Times.Once);
    }
}
