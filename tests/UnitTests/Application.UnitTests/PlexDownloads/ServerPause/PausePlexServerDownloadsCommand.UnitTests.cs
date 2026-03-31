using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class PausePlexServerDownloadsCommandUnitTests : BaseUnitTest<PausePlexServerDownloadsCommandHandler>
{
    [Test]
    public async Task ShouldPauseServerAndStopAllActiveJobs()
    {
        // Arrange
        await SetupDatabase(
            42213,
            config =>
            {
                config.MovieDownloadTasksCount = 1;
                config.TvShowDownloadTasksCount = 1;
                config.TvShowSeasonDownloadTasksCount = 1;
                config.TvShowEpisodeDownloadTasksCount = 1;
            }
        );

        var plexServerId = (await IDbContext.PlexServers.FirstAsync(CancellationToken)).Id;

        var movieFile = await IDbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);

        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        var downloadingKey = movieFile.ToKey();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(plexServerId))
            .ReturnsAsync([downloadingKey]);
        Mock.SetupCommand(It.IsAny<PauseDownloadTaskCommand>).ReturnsAsync(Result.Ok());

        // Act
        var result = await Sut.ExecuteAsync(new PausePlexServerDownloadsCommand(plexServerId), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var server = await IDbContext.PlexServers.GetAsync(plexServerId, CancellationToken);
        server.ShouldNotBeNull();
        server!.IsDownloadsPausedByUser.ShouldBeTrue();

        Mock.Mock<ICommandExecutor>()
            .Verify(
                x =>
                    x.Send(
                        It.Is<PauseDownloadTaskCommand>(c => c.DownloadTaskGuid == downloadingKey.Id),
                        CancellationToken
                    ),
                Times.Once()
            );
    }
}
