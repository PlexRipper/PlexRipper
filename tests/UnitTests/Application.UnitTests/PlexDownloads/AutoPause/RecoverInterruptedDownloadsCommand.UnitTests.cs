namespace Reaparr.Application.UnitTests;

public class RecoverInterruptedDownloadsCommandUnitTests : BaseUnitTest<RecoverInterruptedDownloadsCommandHandler>
{
    [Test]
    public async Task ShouldSetAutoPaused_ForAllDownloadingMovieAndEpisodeFileTasks()
    {
        // Arrange
        await SetupDatabase(
            68111,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.MovieDownloadTasksCount = 2;
                config.TvShowCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 2;
            }
        );

        var movieFile = await IDbContext.DownloadTaskMovieFile.AsTracking().FirstAsync(CancellationToken);
        var episodeFile = await IDbContext.DownloadTaskTvShowEpisodeFile.AsTracking().FirstAsync(CancellationToken);

        movieFile.DownloadStatus = DownloadStatus.Downloading;
        episodeFile.DownloadStatus = DownloadStatus.Downloading;

        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(It.IsAny<DownloadTaskKey>(), It.IsAny<DownloadStatus>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new RecoverInterruptedDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x => x.OnStatusChangedAsync(
                    It.Is<DownloadTaskKey>(k => k.Id == movieFile.Id),
                    DownloadStatus.AutoPaused,
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x => x.OnStatusChangedAsync(
                    It.Is<DownloadTaskKey>(k => k.Id == episodeFile.Id),
                    DownloadStatus.AutoPaused,
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
    }
}
