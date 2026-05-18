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
            .ReturnsAsync([keyA, keyA]); // duplicate key to validate Distinct()

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(serverIds[1]))
            .ReturnsAsync([keyB]);

        Mock.SetupCommand<Result>(c => c is PauseDownloadTaskCommand)
            .Returns(Task.FromResult(Result.Ok()));

        // Act
        var result = await Sut.ExecuteAsync(new AutoPauseActiveDownloadsCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.GetCurrentlyDownloadingKeysByServer(serverIds[0]), Times.Once);
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.GetCurrentlyDownloadingKeysByServer(serverIds[1]), Times.Once);

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(
                    It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == keyA.Id && c.AutoPause),
                    CancellationToken
                ),
                Times.Once
            );

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x => x.Send(
                    It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == keyB.Id && c.AutoPause),
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
            .ReturnsAsync([fileTask.ToKey()]);

        Mock.SetupCommand<Result>(c => c is PauseDownloadTaskCommand)
            .Returns(Task.FromResult(Result.Fail("pause failed")));

        // Act
        var result = await Sut.ExecuteAsync(new AutoPauseActiveDownloadsCommand(), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }
}
