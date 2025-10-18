using Microsoft.EntityFrameworkCore;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class MoveDownloadFileJobQueueUnitTests : BaseUnitTest<MoveDownloadFileJobQueue>
{
    public MoveDownloadFileJobQueueUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldNotRunAnotherFileMoveJob_WhenAJobIsAlreadyRunning()
    {
        // Arrange
        mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        // Act
        var result = await _sut.CheckMoveDownloadFileJobQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotRunFileMoveJob_WhenNoDownloadTaskIsAvailable()
    {
        await SetupDatabase(9, config => config.PlexServerCount = 1);

        // Arrange
        mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await _sut.CheckMoveDownloadFileJobQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
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

        mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await _sut.CheckMoveDownloadFileJobQueue();

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

        mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsAnyMoveDownloadFileJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.StartMoveDownloadFileJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await _sut.CheckMoveDownloadFileJobQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }
}
