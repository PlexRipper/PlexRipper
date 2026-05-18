namespace Reaparr.Application.UnitTests;

public class AutoPauseActiveDownloadsCommandUnitTests : BaseUnitTest<AutoPauseActiveDownloadsCommandHandler>
{
    [Test]
    public async Task ShouldPauseAllUniqueActiveDownloadsAcrossServers_WithAutoPauseFlag()
    {
        // Arrange
        await SetupDatabase(
            68101,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.MovieDownloadTasksCount = 2;
            }
        );

        var serverIds = await IDbContext.PlexServers.AsNoTracking().Select(x => x.Id).ToListAsync(CancellationToken);
        serverIds.Count.ShouldBe(2);

        var fileTasks = await IDbContext.DownloadTaskMovieFile.AsNoTracking().ToListAsync(CancellationToken);
        var keyA = fileTasks[0].ToKey();
        var keyB = fileTasks[1].ToKey();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(serverIds[0]))
            .ReturnsAsync([keyA, keyA])
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(serverIds[1]))
            .ReturnsAsync([keyB])
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.GetCurrentlyMovingKeysByServer(It.IsAny<int>()))
            .ReturnsAsync([])
            .Verifiable(Times.Exactly(4));

        Mock.SetupCommand<Result>(c => c is PauseDownloadTaskCommand)
            .Returns(Task.FromResult(Result.Ok()));

        // Act
        var result = await Sut.ExecuteAsync(new AutoPauseActiveDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IDownloadTaskScheduler>().Verify();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify();

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(
                    It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == keyA.Id && c.AutoPause),
                    CancellationToken
                ),
                Times.Exactly(2)
            );

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(
                    It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == keyB.Id && c.AutoPause),
                    CancellationToken
                ),
                Times.Exactly(2)
            );
    }

    [Test]
    public async Task ShouldPauseActiveMoveKeys_WithAutoPauseFlag()
    {
        // Arrange
        await SetupDatabase(
            68103,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var serverId = await IDbContext.PlexServers.AsNoTracking().Select(x => x.Id).FirstAsync(CancellationToken);
        var moveKey = (await IDbContext.DownloadTaskMovieFile.AsNoTracking().FirstAsync(CancellationToken)).ToKey();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(serverId))
            .ReturnsAsync([])
            .Verifiable(Times.Exactly(2));

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.GetCurrentlyMovingKeysByServer(serverId))
            .ReturnsAsync([moveKey])
            .Verifiable(Times.Exactly(2));

        Mock.SetupCommand<Result>(c => c is PauseDownloadTaskCommand)
            .Returns(Task.FromResult(Result.Ok()));

        // Act
        var result = await Sut.ExecuteAsync(new AutoPauseActiveDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>().Verify();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(
                    It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == moveKey.Id && c.AutoPause),
                    CancellationToken
                ),
                Times.Exactly(2)
            );
    }

    [Test]
    public async Task ShouldRunSecondSnapshotPass_WhenLateArrivalAppears()
    {
        // Arrange
        await SetupDatabase(
            68104,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.MovieDownloadTasksCount = 2;
            }
        );

        var serverId = await IDbContext.PlexServers.AsNoTracking().Select(x => x.Id).FirstAsync(CancellationToken);
        var fileTasks = await IDbContext.DownloadTaskMovieFile.AsNoTracking().ToListAsync(CancellationToken);
        var firstPassKey = fileTasks[0].ToKey();
        var secondPassKey = fileTasks[1].ToKey();

        Mock.Mock<IDownloadTaskScheduler>()
            .SetupSequence(x => x.GetCurrentlyDownloadingKeysByServer(serverId))
            .ReturnsAsync([firstPassKey])
            .ReturnsAsync([secondPassKey]);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.GetCurrentlyMovingKeysByServer(serverId))
            .ReturnsAsync([])
            .Verifiable(Times.Exactly(2));

        Mock.SetupCommand<Result>(c => c is PauseDownloadTaskCommand)
            .Returns(Task.FromResult(Result.Ok()));

        // Act
        var result = await Sut.ExecuteAsync(new AutoPauseActiveDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(
                    It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == firstPassKey.Id && c.AutoPause),
                    CancellationToken
                ),
                Times.Once
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(
                    It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == secondPassKey.Id && c.AutoPause),
                    CancellationToken
                ),
                Times.Once
            );
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenPauseCommandFails()
    {
        // Arrange
        await SetupDatabase(
            68102,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var serverId = await IDbContext.PlexServers.AsNoTracking().Select(x => x.Id).FirstAsync(CancellationToken);
        var fileTask = await IDbContext.DownloadTaskMovieFile.AsNoTracking().FirstAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(serverId))
            .ReturnsAsync([fileTask.ToKey()])
            .Verifiable(Times.Once);

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.GetCurrentlyMovingKeysByServer(serverId))
            .ReturnsAsync([])
            .Verifiable(Times.Once);

        Mock.SetupCommand<Result>(c => c is PauseDownloadTaskCommand)
            .Returns(Task.FromResult(Result.Fail("pause failed")));

        // Act
        var result = await Sut.ExecuteAsync(new AutoPauseActiveDownloadsCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IDownloadTaskScheduler>().Verify();
        Mock.Mock<IMoveDownloadFileScheduler>().Verify();
    }
}
