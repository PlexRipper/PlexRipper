using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class DeleteDownloadTaskEndpointUnitTests : BaseUnitTest<DeleteDownloadTaskEndpoint>
{
    public DeleteDownloadTaskEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldDispatchDeleteCommand_WhenDownloadTaskIdIsGiven()
    {
        // Arrange
        await SetupDatabase(
            45210,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var episodeFileId = await dbContext
            .DownloadTaskTvShowEpisodeFile.Select(x => x.Id)
            .SingleAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var ep = SetupEndpointUnitTest<DeleteDownloadTaskEndpoint>();
        await ep.HandleAsync(
            new DeleteDownloadTaskEndpointRequest { DownloadTaskIds = [episodeFileId] },
            CancellationToken
        );
        var result = ep.Response;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<DeleteDownloadTasksByKeyCommand>(cmd => cmd.Keys.Any(k => k.Id == episodeFileId)),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<StopDownloadTaskCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldStopDownloadingBeforeDispatching_WhenTaskIsActivelyDownloading()
    {
        // Arrange
        await SetupDatabase(
            45211,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieId = await dbContext.DownloadTaskMovie.Select(x => x.Id).SingleAsync(CancellationToken);

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<StopDownloadTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<DeleteDownloadTasksByKeyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var ep = SetupEndpointUnitTest<DeleteDownloadTaskEndpoint>();
        await ep.HandleAsync(new DeleteDownloadTaskEndpointRequest { DownloadTaskIds = [movieId] }, CancellationToken);
        var result = ep.Response;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<StopDownloadTaskCommand>(cmd => cmd.DownloadTaskGuid == movieId),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<DeleteDownloadTasksByKeyCommand>(cmd => cmd.Keys.Any(k => k.Id == movieId)),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
    }
}
