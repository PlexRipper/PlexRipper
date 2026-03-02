using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Identity.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.Application.UnitTests;

public class DeleteDownloadTaskEndpointUnitTests : BaseUnitTest
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
        var ep = Factory.Create<DeleteDownloadTaskEndpoint>(ctx =>
        {
            ctx.AddTestServices(s =>
            {
                s.AddTransient(_ => Mock.Create<ILogger>());
                s.AddTransient(_ => Mock.Create<IReaparrDbContext>());
                s.AddTransient(_ => Mock.Create<IAuthDbContext>());
                s.AddTransient(_ => Mock.Create<IAuthDbContextFactory>());
                s.AddTransient(_ => Mock.Mock<ICommandExecutor>().Object);
                s.AddSingleton(_ => Mock.Create<ISchedulerService>());
                s.AddSingleton(_ => Mock.Mock<IProgressHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<IDownloadHubService>().Object);
                s.AddSingleton(_ => Mock.Mock<INotificationHubService>().Object);
                s.AddTransient(_ => Mock.Mock<IDownloadTaskScheduler>().Object);
            });
        });

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
    }
}
