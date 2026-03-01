using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Domain;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application.UnitTests;

public class PausePlexServerDownloadsCommandUnitTests : BaseUnitTest<PausePlexServerDownloadsCommandHandler>
{
    public PausePlexServerDownloadsCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
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
        var episodeFile = await IDbContext.DownloadTaskTvShowEpisodeFile.FirstAsync(CancellationToken);

        await IDbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        await IDbContext
            .DownloadTaskTvShowEpisodeFile.Where(x => x.Id == episodeFile.Id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadStatus, DownloadStatus.Moving), CancellationToken);

        var downloadingKey = movieFile.ToKey();
        var movingKey = episodeFile.ToKey();

        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.GetCurrentlyDownloadingKeysByServer(plexServerId))
            .ReturnsAsync([downloadingKey]);
        Mock.Mock<IDownloadTaskScheduler>()
            .Setup(x => x.StopDownloadTaskJob(downloadingKey, It.IsAny<CancellationToken>()))
            .ReturnOk();

        Mock.Mock<IMoveDownloadFileScheduler>()
            .Setup(x => x.IsDownloadFileMoving(It.IsAny<DownloadTaskKey>()))
            .ReturnsAsync(true);
        Mock.Mock<IMoveDownloadFileScheduler>().Setup(x => x.StopMoveDownloadFileJob(movingKey)).ReturnOk();

        // Act
        var result = await Sut.ExecuteAsync(new PausePlexServerDownloadsCommand(plexServerId), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var server = await IDbContext.PlexServers.GetAsync(plexServerId, CancellationToken);
        server.ShouldNotBeNull();
        server!.IsDownloadsPausedByUser.ShouldBeTrue();

        var updatedMovieFile = await IDbContext.DownloadTaskMovieFile.GetAsync(movieFile.Id, CancellationToken);
        var updatedEpisodeFile = await IDbContext.DownloadTaskTvShowEpisodeFile.GetAsync(
            episodeFile.Id,
            CancellationToken
        );

        updatedMovieFile!.DownloadStatus.ShouldBe(DownloadStatus.Paused);
        updatedEpisodeFile!.DownloadStatus.ShouldBe(DownloadStatus.MovePaused);

        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.StopDownloadTaskJob(downloadingKey, It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IMoveDownloadFileScheduler>().Verify(x => x.StopMoveDownloadFileJob(movingKey), Times.Once);
    }
}
