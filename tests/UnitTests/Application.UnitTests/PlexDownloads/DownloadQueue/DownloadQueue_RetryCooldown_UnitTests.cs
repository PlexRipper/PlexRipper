namespace Reaparr.Application.UnitTests;

public class DownloadQueueRetryCooldownUnitTests : BaseUnitTest<DownloadQueue>
{
    [Test]
    public async Task ShouldSkipRecentlyAttemptedTask_AndPickAnotherInsteadOnRapidReentry()
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
}
