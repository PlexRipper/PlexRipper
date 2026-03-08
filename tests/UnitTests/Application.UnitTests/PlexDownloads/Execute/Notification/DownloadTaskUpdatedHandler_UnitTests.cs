using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadTaskUpdatedHandlerUnitTests : BaseUnitTest<DownloadTaskUpdatedHandler>
{
    public DownloadTaskUpdatedHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldQueueProgressUpdate_WhenDownloadTaskUpdatedHasBeenCalled()
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

        Mock.Mock<IDownloadPatchBroadcaster>()
            .Setup(x => x.TryMarkStatusChanged(It.IsAny<Guid>(), It.IsAny<DownloadStatus>()))
            .Returns(false)
            .Verifiable(Times.Once);

        Mock.Mock<IDownloadPatchBroadcaster>()
            .Setup(x =>
                x.MarkProgressDirtyAsync(
                    It.IsAny<int>(),
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        // Act
        var command = new DownloadTaskUpdatedCommand(downloadTasks[0].ToKey());
        await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        Mock.Mock<IDownloadPatchBroadcaster>()
            .Verify(
                x =>
                    x.MarkProgressDirtyAsync(
                        It.IsAny<int>(),
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }

    [Fact]
    public async Task ShouldQueueImmediateStatusPatch_WhenDownloadTaskStatusChanged()
    {
        // Arrange
        await SetupDatabase(81983, config => config.MovieDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken);
        var updatedDownloadTask = downloadTasks[0].Children[0];
        await IDbContext.SetDownloadStatus(updatedDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        Mock.Mock<IDownloadPatchBroadcaster>()
            .Setup(x => x.TryMarkStatusChanged(It.IsAny<Guid>(), It.IsAny<DownloadStatus>()))
            .Returns(true);

        Mock.Mock<IDownloadPatchBroadcaster>()
            .Setup(x =>
                x.PublishImmediateStatusPatchAsync(
                    It.IsAny<int>(),
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        Mock.Mock<IDownloadPatchBroadcaster>()
            .Setup(x =>
                x.MarkProgressDirtyAsync(
                    It.IsAny<int>(),
                    It.IsAny<DownloadTaskKey>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        // Act
        var command = new DownloadTaskUpdatedCommand(downloadTasks[0].ToKey());
        await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        Mock.Mock<IDownloadPatchBroadcaster>()
            .Verify(
                x =>
                    x.PublishImmediateStatusPatchAsync(
                        It.IsAny<int>(),
                        It.IsAny<DownloadTaskKey>(),
                        It.IsAny<IReadOnlyCollection<Guid>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
