using Microsoft.EntityFrameworkCore;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class FileMergeQueueUnitTests : BaseUnitTest<MoveDownloadFileJobQueue>
{
    public FileMergeQueueUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldNotRunAnotherFileMergeJob_WhenAJobIsAlreadyRunning()
    {
        // Arrange
        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.IsAnyFileMergeJobRunning())
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        // Act
        var result = await _sut.CheckFileMergeQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotRunFileMergeJob_WhenNoDownloadTaskIsAvailable()
    {
        await SetupDatabase(9, config => config.PlexServerCount = 1);

        // Arrange
        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.IsAnyFileMergeJobRunning())
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.StartFileMergeJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never);

        // Act
        var result = await _sut.CheckFileMergeQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldRunAFileMergeJob_WhenDownloadTaskMovieFileWithDownloadFinishedExist()
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

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.IsAnyFileMergeJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.StartFileMergeJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await _sut.CheckFileMergeQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRunAFileMergeJob_WhenDownloadTaskTvShowEpisodeFileWithDownloadFinishedExist()
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

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.IsAnyFileMergeJobRunning())
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.StartFileMergeJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once);

        // Act
        var result = await _sut.CheckFileMergeQueue();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
    }
}
