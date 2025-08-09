using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;

namespace PlexRipper.Application.UnitTests;

public class DownloadTaskUpdatedHandler_UnitTests : BaseUnitTest<DownloadTaskUpdatedHandler>
{
    public DownloadTaskUpdatedHandler_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSendDownloadTasksWithSignalR_WhenDownloadTaskUpdatedHasBeenCalled()
    {
        // Arrange
        await SetupDatabase(
            34292,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.TvShowDownloadTasksCount = 5;
                config.TvShowSeasonDownloadTasksCount = 5;
                config.TvShowEpisodeDownloadTasksCount = 5;
            }
        );

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken);

        mock.Mock<ISignalRService>()
            .Setup(x =>
                x.SendDownloadProgressUpdateAsync(It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        // Act
        var command = new DownloadTaskUpdatedNotification(downloadTasks[0].ToKey());
        await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        mock.Mock<ISignalRService>()
            .Verify(
                x =>
                    x.SendDownloadProgressUpdateAsync(
                        It.IsAny<List<DownloadTaskGeneric>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Fact]
    public async Task ShouldStartFileMergeJobAndDownloadQueue_WhenDownloadTaskHasFinished()
    {
        // Arrange
        await SetupDatabase(81983, config => config.MovieDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken);
        var updatedDownloadTask = downloadTasks[0].Children[0];
        await IDbContext.SetDownloadStatus(updatedDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        mock.Mock<ISignalRService>()
            .Setup(x =>
                x.SendDownloadProgressUpdateAsync(It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        mock.Mock<IFileMergeScheduler>()
            .Setup(x => x.StartFileMergeJob(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var command = new DownloadTaskUpdatedNotification(downloadTasks[0].ToKey());
        await _sut.ExecuteAsync(command, CancellationToken);

        // Assert
        mock.Mock<ISignalRService>()
            .Verify(
                x =>
                    x.SendDownloadProgressUpdateAsync(
                        It.IsAny<List<DownloadTaskGeneric>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
