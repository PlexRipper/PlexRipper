namespace Reaparr.Application.UnitTests;

public class DownloadQueueCheckDownloadQueueForAllServersUnitTests : BaseUnitTest<DownloadQueue>
{
    [Test]
    public async Task ShouldKickQueueForConfiguredPlexServers_WhenCheckingAllServers()
    {
        // Arrange
        await SetupDatabase(
            58121,
            config =>
            {
                config.PlexServerCount = 2;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 2;
                config.MovieDownloadTasksCount = 2;
            }
        );

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>(), CancellationToken))
            .ReturnOk();
        Mock.Mock<IDownloadTaskScheduler>().Setup(x => x.IsServerDownloading(It.IsAny<int>())).ReturnsAsync(false);

        Sut.Setup(CancellationToken);

        // Act
        var result = await Sut.CheckDownloadQueueForAllServers(CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await ShouldEventuallyAsync(() =>
        {
            Mock.Mock<IDownloadTaskScheduler>()
                .Verify(x => x.StartDownloadTaskJob(It.IsAny<DownloadTaskKey>(), CancellationToken), Times.Once);
        });
    }

    private static async Task ShouldEventuallyAsync(Action assertion)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(5);
        Exception? lastException = null;

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                assertion();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                await Task.Delay(50);
            }
        }

        if (lastException is not null)
            throw lastException;
    }
}
