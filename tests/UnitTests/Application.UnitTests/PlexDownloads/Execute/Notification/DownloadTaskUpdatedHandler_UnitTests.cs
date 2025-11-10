using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class DownloadTaskUpdatedHandlerUnitTests : BaseUnitTest<DownloadTaskUpdatedHandler>
{
    public DownloadTaskUpdatedHandlerUnitTests(ITestOutputHelper output)
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

        Mock.Mock<ISignalRService>()
            .Setup(x =>
                x.SendDownloadProgressUpdateAsync(It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        // Act
        var command = new DownloadTaskUpdatedCommand(downloadTasks[0].ToKey());
        await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        Mock.Mock<ISignalRService>()
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
    public async Task ShouldStartMoveDownloadJobAndDownloadQueue_WhenDownloadTaskHasFinished()
    {
        // Arrange
        await SetupDatabase(81983, config => config.MovieDownloadTasksCount = 5);

        var downloadTasks = await IDbContext.GetAllDownloadTasksByServerAsync(cancellationToken: CancellationToken);
        var updatedDownloadTask = downloadTasks[0].Children[0];
        await IDbContext.SetDownloadStatus(updatedDownloadTask.ToKey(), DownloadStatus.DownloadFinished);

        Mock.Mock<ISignalRService>()
            .Setup(x =>
                x.SendDownloadProgressUpdateAsync(It.IsAny<List<DownloadTaskGeneric>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        // Act
        var command = new DownloadTaskUpdatedCommand(downloadTasks[0].ToKey());
        await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        Mock.Mock<ISignalRService>()
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
