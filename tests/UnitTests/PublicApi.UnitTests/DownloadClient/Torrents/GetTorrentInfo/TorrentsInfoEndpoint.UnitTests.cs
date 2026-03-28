using Microsoft.EntityFrameworkCore;

namespace Reaparr.PublicAPI.UnitTests;

public class TorrentsInfoEndpointUnitTests : BaseUnitTest<TorrentsInfoEndpoint>
{
    [Test]
    public async Task ShouldReturnRatioLimitZero_WhenStatusIsCompleted()
    {
        // Arrange — Radarr only sets CanBeRemoved when HasReachedSeedLimit() is true.
        // ratio_limit=0 with ratio=0 satisfies that check so DELETE is triggered after import.
        await SetupDatabase(
            5101,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, "hash-completed")
                        .SetProperty(p => p.DownloadStatus, DownloadStatus.Completed),
                CancellationToken
            );

        var request = new TorrentsInfoEndpointRequest();

        // Act
        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(0);
    }

    [Test]
    public async Task ShouldReturnRatioLimitZero_WhenStatusIsMoveFinished()
    {
        await SetupDatabase(
            5102,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, "hash-move-finished")
                        .SetProperty(p => p.DownloadStatus, DownloadStatus.MoveFinished),
                CancellationToken
            );

        var request = new TorrentsInfoEndpointRequest();

        // Act
        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(0);
    }

    [Test]
    public async Task ShouldReturnRatioLimitZero_WhenStatusIsDownloadFinished()
    {
        await SetupDatabase(
            5103,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, "hash-download-finished")
                        .SetProperty(p => p.DownloadStatus, DownloadStatus.DownloadFinished),
                CancellationToken
            );

        var request = new TorrentsInfoEndpointRequest();

        // Act
        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(0);
    }

    [Test]
    public async Task ShouldReturnRatioLimitMinusTwo_WhenStatusIsDownloading()
    {
        // Arrange — active downloads must not be flagged as ready for removal.
        await SetupDatabase(
            5104,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, "hash-downloading")
                        .SetProperty(p => p.DownloadStatus, DownloadStatus.Downloading),
                CancellationToken
            );

        var request = new TorrentsInfoEndpointRequest();

        // Act
        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(-2);
    }

    [Test]
    public async Task ShouldReturnRatioLimitMinusTwo_WhenStatusIsMovePaused()
    {
        // Arrange — a paused mid-move must not be flagged as ready for removal.
        await SetupDatabase(
            5105,
            config =>
            {
                config.PlexServerCount = 1;
                config.MovieCount = 1;
                config.MovieDownloadTasksCount = 1;
            }
        );

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);
        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(p => p.HashId, "hash-move-paused")
                        .SetProperty(p => p.DownloadStatus, DownloadStatus.MovePaused),
                CancellationToken
            );

        var request = new TorrentsInfoEndpointRequest();

        // Act
        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);
        var response = endpoint.Response;

        // Assert
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(-2);
    }
}
