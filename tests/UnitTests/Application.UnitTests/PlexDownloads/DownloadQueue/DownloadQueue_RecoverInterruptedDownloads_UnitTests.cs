namespace Reaparr.Application.UnitTests;

public class DownloadQueueRecoverInterruptedDownloadsUnitTests : BaseUnitTest<DownloadQueue>
{
    [Test]
    public async Task ShouldResetDownloadingLeavesToQueued_WhenSomeAreLeftInDownloadingFromAPreviousRun()
    {
        // Arrange
        await SetupDatabase(
            41207,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 4;
                config.MovieDownloadTasksCount = 3;
            }
        );

        var dbContext = IDbContext;
        var downloadTasks = await dbContext
            .DownloadTaskMovie.AsTracking()
            .Where(x => x.PlexServerId == 1)
            .IncludeAll()
            .ToListAsync(CancellationToken);
        downloadTasks[0].SetDownloadStatus(DownloadStatus.Downloading);
        downloadTasks[1].SetDownloadStatus(DownloadStatus.Downloading);
        await dbContext.SaveChangesAsync(CancellationToken);
        var zombieLeafIds = new[]
        {
            downloadTasks[0].Children.First().Id,
            downloadTasks[1].Children.First().Id,
        };

        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Setup(x =>
                x.OnStatusChangedAsync(It.IsAny<DownloadTaskKey>(), It.IsAny<DownloadStatus>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.RecoverInterruptedDownloadsAsync(CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        foreach (var zombieLeafId in zombieLeafIds)
        {
            Mock.Mock<IDownloadTaskUpdateDispatcher>()
                .Verify(
                    x => x.OnStatusChangedAsync(
                        It.Is<DownloadTaskKey>(k => k.Id == zombieLeafId),
                        DownloadStatus.Queued,
                        It.IsAny<CancellationToken>()
                    ),
                    Times.Once()
                );
        }
    }

    [Test]
    public async Task ShouldDoNothing_WhenNoTasksAreInDownloadingStatus()
    {
        // Arrange
        await SetupDatabase(
            41208,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 3;
                config.MovieDownloadTasksCount = 3;
            }
        );

        // Act
        var result = await Sut.RecoverInterruptedDownloadsAsync(CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IDownloadTaskUpdateDispatcher>()
            .Verify(
                x => x.OnStatusChangedAsync(It.IsAny<DownloadTaskKey>(), It.IsAny<DownloadStatus>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
    }
}
