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
        var result = await PrepareAndExecuteTorrentsInfoTest(5102, "hash-move-finished", DownloadStatus.MoveFinished);

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
        var result = await PrepareAndExecuteTorrentsInfoTest(
            5103,
            "hash-download-finished",
            DownloadStatus.DownloadFinished
        );

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
        // Arrange — active downloads must not be flagged as ready for removal.
        var result = await PrepareAndExecuteTorrentsInfoTest(5104, "hash-downloading", DownloadStatus.Downloading);

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
        // Arrange — a paused mid-move must not be flagged as ready for removal.
        var result = await PrepareAndExecuteTorrentsInfoTest(5105, "hash-move-paused", DownloadStatus.MovePaused);

        var response = result.Response;
        response.ShouldNotBeNull();
        response.Count.ShouldBe(1);
        response[0].RatioLimit.ShouldBe(-2);
        response[0].State.ShouldBe("pausedUP");
        result.PersistedStatus.ShouldBe(DownloadStatus.MovePaused);
        result.PersistedHash.ShouldBe("hash-move-paused");
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
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .ExecuteUpdateAsync(
                x => x.SetProperty(p => p.HashId, hash).SetProperty(p => p.DownloadStatus, downloadStatus),
                CancellationToken
            );

        var endpoint = SetupEndpointUnitTest<TorrentsInfoEndpoint>();
        await endpoint.HandleAsync(new TorrentsInfoEndpointRequest(), CancellationToken);

        var persistedRow = await dbContext
            .DownloadTaskMovieFile.Where(x => x.Id == movieFile.Id)
            .Select(x => new { x.DownloadStatus, x.HashId })
            .FirstAsync(CancellationToken);

        return (endpoint.Response, persistedRow.DownloadStatus, persistedRow.HashId);
    }
}
