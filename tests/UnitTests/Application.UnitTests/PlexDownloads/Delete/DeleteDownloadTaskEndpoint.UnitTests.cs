using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

public class DeleteDownloadTaskEndpointUnitTests : BaseUnitTest<DeleteDownloadTaskEndpoint>
{
    public DeleteDownloadTaskEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldRemoveOrphanedTvShowParents_WhenLastEpisodeFileIsDeletedById()
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
            .ReturnsAsync(false);

        // Act
        var ep = SetupEndpointUnitTest<DeleteDownloadTaskEndpoint>();
        await ep.HandleAsync(
            new DeleteDownloadTaskEndpointRequest { DownloadTaskIds = [episodeFileId] },
            CancellationToken
        );
        var result = ep.Response;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await dbContext.DownloadTaskTvShow.ToListAsync(CancellationToken)).ShouldBeEmpty();
        (await dbContext.DownloadTaskTvShowSeason.ToListAsync(CancellationToken)).ShouldBeEmpty();
        (await dbContext.DownloadTaskTvShowEpisode.ToListAsync(CancellationToken)).ShouldBeEmpty();
        (await dbContext.DownloadTaskTvShowEpisodeFile.ToListAsync(CancellationToken)).ShouldBeEmpty();
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(x => x.IsDownloading(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>()), Times.Once);
        Mock.Mock<IDownloadTaskScheduler>()
            .Verify(
                x =>
                    x.StopDownloadTaskJob(It.IsAny<DownloadTaskKey>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()),
                Times.Never
            );
    }
}
