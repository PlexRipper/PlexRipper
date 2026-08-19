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

        var movieFile = await IDbContext.DownloadTaskMovieFile.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        var episodeFile = await IDbContext
            .DownloadTaskTvShowEpisodeFile.OrderBy(x => x.Id)
            .FirstAsync(CancellationToken);

        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );
        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == episodeFile.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await Sut.ExecuteAsync(new RecoverInterruptedDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == movieFile.Id),
                        DownloadStatus.AutoPaused,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == episodeFile.Id),
                        DownloadStatus.AutoPaused,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(2)
            );
    }

    [Test]
    public async Task ShouldSetAutoMovePaused_ForAllMovingMovieAndEpisodeFileTasks()
    {
        // Arrange
        await SetupDatabase(
            68112,
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

        var movieFile = await IDbContext.DownloadTaskMovieFile.OrderBy(x => x.Id).FirstAsync(CancellationToken);
        var episodeFile = await IDbContext
            .DownloadTaskTvShowEpisodeFile.OrderBy(x => x.Id)
            .FirstAsync(CancellationToken);

        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Moving), CancellationToken);
        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == episodeFile.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Moving), CancellationToken);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await Sut.ExecuteAsync(new RecoverInterruptedDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == movieFile.Id),
                        DownloadStatus.AutoMovePaused,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == episodeFile.Id),
                        DownloadStatus.AutoMovePaused,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(2)
            );
    }

    [Test]
    public async Task ShouldSetCompleted_ForMoveFinishedMovieAndEpisodeFileTasks()
    {
        // Arrange
        await SetupDatabase(
            68115,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
                config.TvShowCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var movieFile = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        var episodeFile = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstAsync(CancellationToken);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.MoveFinished),
                CancellationToken
            );
        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == episodeFile.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.MoveFinished),
                CancellationToken
            );

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    DownloadStatus.Completed,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.ExecuteAsync(new RecoverInterruptedDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == movieFile.Id),
                        DownloadStatus.Completed,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == episodeFile.Id),
                        DownloadStatus.Completed,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Test]
    public async Task ShouldOnlyRecoverActiveStatuses_WhenMixedStatusesExist()
    {
        // Arrange
        await SetupDatabase(
            68113,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 4;
            }
        );

        var movieFiles = await IDbContext
            .DownloadTaskMovieFile.OrderBy(x => x.Id)
            .Take(4)
            .ToListAsync(CancellationToken);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFiles[0].Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFiles[1].Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Moving), CancellationToken);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFiles[2].Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Paused), CancellationToken);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFiles[3].Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Completed), CancellationToken);

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<DownloadStatus>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Exactly(2));

        // Act
        var result = await Sut.ExecuteAsync(new RecoverInterruptedDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == movieFiles[0].Id),
                        DownloadStatus.AutoPaused,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == movieFiles[1].Id),
                        DownloadStatus.AutoMovePaused,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == movieFiles[2].Id || k.Id == movieFiles[3].Id),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(2)
            );
    }

    [Test]
    public async Task ShouldNotDispatchStatusChanges_WhenNoRecoverableStatusesExist()
    {
        // Arrange
        await SetupDatabase(
            68114,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 2;
            }
        );

        var movieFiles = await IDbContext.DownloadTaskMovieFile.OrderBy(x => x.Id).ToListAsync(CancellationToken);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFiles[0].Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Paused), CancellationToken);
        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFiles[1].Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.MovePaused),
                CancellationToken
            );

        // Act
        var result = await Sut.ExecuteAsync(new RecoverInterruptedDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x =>
                    x.OnStatusChangedAsync(
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<DownloadStatus>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
    }
}
