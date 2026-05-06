using Reaparr.Data.Contracts;
using Reaparr.PublicAPI.Contracts;

namespace Reaparr.PublicAPI.UnitTests;

public class TorrentsInfoEndpointUnitTests : BaseUnitTest<TorrentsInfoEndpoint>
{
    [Test]
    public async Task ShouldReturnRatioLimitZero_WhenStatusIsCompleted()
    {
        // Arrange — Radarr only sets CanBeRemoved when HasReachedSeedLimit() is true.
        // ratio_limit=0 with ratio=0 satisfies that check so DELETE is triggered after import.
        var result = await PrepareAndExecuteTorrentsInfoTest(5101, "hash-completed", DownloadStatus.Completed);

        // Assert
        var response = result.Response;
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(0);
        response[0].State.ShouldBe("pausedUP");
        result.PersistedStatus.ShouldBe(DownloadStatus.Completed);
        result.PersistedHash.ShouldBe("hash-completed");
    }

    [Test]
    public async Task ShouldReturnRatioLimitZero_WhenStatusIsMoveFinished()
    {
        // Arrange
        var result = await PrepareAndExecuteTorrentsInfoTest(5102, "hash-move-finished", DownloadStatus.MoveFinished);

        // Assert
        var response = result.Response;
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(0);
        response[0].State.ShouldBe("pausedUP");
        result.PersistedStatus.ShouldBe(DownloadStatus.MoveFinished);
        result.PersistedHash.ShouldBe("hash-move-finished");
    }

    [Test]
    public async Task ShouldReturnRatioLimitZero_WhenStatusIsDownloadFinished()
    {
        // Arrange
        var result = await PrepareAndExecuteTorrentsInfoTest(
            5103,
            "hash-download-finished",
            DownloadStatus.DownloadFinished
        );

        // Assert
        var response = result.Response;
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(0);
        response[0].State.ShouldBe("pausedUP");
        result.PersistedStatus.ShouldBe(DownloadStatus.DownloadFinished);
        result.PersistedHash.ShouldBe("hash-download-finished");
    }

    [Test]
    public async Task ShouldReturnRatioLimitMinusTwo_WhenStatusIsDownloading()
    {
        // Arrange
        // active downloads must not be flagged as ready for removal.
        var result = await PrepareAndExecuteTorrentsInfoTest(5104, "hash-downloading", DownloadStatus.Downloading);

        // Assert
        var response = result.Response;
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(-2);
        response[0].State.ShouldBe("downloading");
        result.PersistedStatus.ShouldBe(DownloadStatus.Downloading);
        result.PersistedHash.ShouldBe("hash-downloading");
    }

    [Test]
    public async Task ShouldReturnRatioLimitMinusTwo_WhenStatusIsMovePaused()
    {
        // Arrange
        // a paused mid-move must not be flagged as ready for removal.
        var result = await PrepareAndExecuteTorrentsInfoTest(5105, "hash-move-paused", DownloadStatus.MovePaused);

        // Assert
        var response = result.Response;
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(-2);
        response[0].State.ShouldBe("pausedUP");
        result.PersistedStatus.ShouldBe(DownloadStatus.MovePaused);
        result.PersistedHash.ShouldBe("hash-move-paused");
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenServerIsOwned()
    {
        // Arrange
        await SetupDatabase(5106, config =>
        {
            config.PlexServerCount = 1;
            config.MovieCount = 1;
            config.MovieDownloadTasksCount = 1;
        });

        var dbContext = IDbContext;
        var movieFile = await dbContext.DownloadTaskMovieFile.FirstAsync(CancellationToken);

        await dbContext
            .PlexServers.Where(x => x.Id == movieFile.PlexServerId)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.IsEnabled, true).SetProperty(p => p.OwnedOverride, true),
                CancellationToken
            );

        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.HashId, "hash-owned").SetProperty(p => p.DownloadStatus, DownloadStatus.Completed),
                CancellationToken
            );

        // Act
        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(
            new TorrentsInfoEndpointRequest
            {
                Hashes = "hash-owned",
                Category = IntegrationDefinitions.RADARR_DEFAULT_CATEGORY,
            },
            CancellationToken
        );

        // Assert
        endpoint.Response.ShouldNotBeNull();
        endpoint.Response.ShouldBeEmpty();

        var persisted = await dbContext.DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .Select(x => new { x.HashId, x.DownloadStatus })
            .FirstAsync(CancellationToken);
        persisted.HashId.ShouldBe("hash-owned");
        persisted.DownloadStatus.ShouldBe(DownloadStatus.Completed);

        var server = await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(x => x.Id == movieFile.PlexServerId, CancellationToken);
        server.IsEnabled.ShouldBeTrue();
        server.OwnedOverride.ShouldBe(true);
    }

    private async Task<(
        List<QBittorrentTorrentInfo> Response,
        DownloadStatus PersistedStatus,
        string? PersistedHash
    )> PrepareAndExecuteTorrentsInfoTest(int seed, string hash, DownloadStatus downloadStatus)
    {
        await SetupDatabase(
            seed,
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
            .PlexServers.Where(x => x.Id == movieFile.PlexServerId)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.IsEnabled, true).SetProperty(p => p.OwnedOverride, false),
                CancellationToken
            );

        await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.HashId, hash).SetProperty(p => p.DownloadStatus, downloadStatus),
                CancellationToken
            );

        // Act
        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(
            new TorrentsInfoEndpointRequest
            {
                Hashes = hash,
                Category = IntegrationDefinitions.RADARR_DEFAULT_CATEGORY,
            },
            CancellationToken
        );

        var persistedRow = await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .Select(x => new { x.DownloadStatus, x.HashId })
            .FirstAsync(CancellationToken);

        return (endpoint.Response, persistedRow.DownloadStatus, persistedRow.HashId);
    }
}
