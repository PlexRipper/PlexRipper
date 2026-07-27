namespace Reaparr.Application.UnitTests;

public class DownloadQueueRetryCooldownUnitTests : BaseUnitTest<DownloadQueue>
{
    [Test]
    public async Task ShouldSkipRecentlyAttemptedQueuedTask_AndPickAnotherInsteadOnRapidReentry()
    {
        // Arrange
        await SetupDatabase(
            58119,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.MovieDownloadTasksCount = 3;
            }
        );

        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>())).ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var firstPick = await Sut.CheckDownloadQueueServer(1);
        var secondPick = await Sut.CheckDownloadQueueServer(1);

        // Assert
        firstPick.IsSuccess.ShouldBeTrue();
        firstPick.Value.ShouldNotBeNull();
        secondPick.IsSuccess.ShouldBeTrue();
        secondPick.Value.ShouldNotBeNull();
        secondPick.Value.Id.ShouldNotBe(firstPick.Value.Id);
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Exactly(2));
    }

    [Test]
    public async Task ShouldSkipRecentlyAttemptedErrorTask_AndPickNextErrorTaskBeforeQueuedTask()
    {
        // Arrange
        await SetupDatabase(
            58120,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.MovieDownloadTasksCount = 3;
            }
        );

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(
            asTracking: true,
            cancellationToken: CancellationToken
        );

        var firstErrorTask = downloadTasks[0].Children[0];
        var secondErrorTask = downloadTasks[1].Children[0];
        var queuedTask = downloadTasks[2].Children[0];

        firstErrorTask.SetDownloadStatus(DownloadStatus.Error);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Error);

        secondErrorTask.SetDownloadStatus(DownloadStatus.Error);
        downloadTasks[1].SetDownloadStatus(DownloadStatus.Error);

        queuedTask.SetDownloadStatus(DownloadStatus.Queued);
        downloadTasks[2].SetDownloadStatus(DownloadStatus.Queued);

        await IDbContext.SaveChangesAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>())).ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var firstPick = await Sut.CheckDownloadQueueServer(1);
        var secondPick = await Sut.CheckDownloadQueueServer(1);

        // Assert
        firstPick.IsSuccess.ShouldBeTrue();
        firstPick.Value.ShouldNotBeNull();
        firstPick.Value.Id.ShouldBe(firstErrorTask.Id);

        secondPick.IsSuccess.ShouldBeTrue();
        secondPick.Value.ShouldNotBeNull();
        secondPick.Value.Id.ShouldBe(secondErrorTask.Id);
        secondPick.Value.Id.ShouldNotBe(queuedTask.Id);

        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>()), Times.Exactly(2));
    }
}
