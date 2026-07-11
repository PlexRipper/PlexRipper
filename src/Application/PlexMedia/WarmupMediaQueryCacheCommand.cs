namespace Reaparr.Application;

public record WarmupMediaQueryCacheCommand : ICommand<Result>;

public class WarmupMediaQueryCacheCommandValidator : AbstractValidator<WarmupMediaQueryCacheCommand>
{
    public WarmupMediaQueryCacheCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class WarmupMediaQueryCacheCommandHandler : ICommandHandler<WarmupMediaQueryCacheCommand, Result>
{
    private static readonly TimeSpan _librarySyncPollInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan _librarySyncQuietPeriod = TimeSpan.FromMinutes(5);

    private readonly ILogger _log;
    private readonly IMediaQueryCache _mediaQueryCache;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public WarmupMediaQueryCacheCommandHandler(
        ILogger log,
        IMediaQueryCache mediaQueryCache,
        IReaparrDbContextFactory dbContextFactory
    )
    {
        _log = log.ForContext<WarmupMediaQueryCacheCommandHandler>();
        _mediaQueryCache = mediaQueryCache;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Quiet period after syncs settle. Overridable for tests to avoid real delays.
    /// </summary>
    protected virtual TimeSpan SyncQuietPeriod { get; set; } = _librarySyncQuietPeriod;

    public async Task<Result> ExecuteAsync(WarmupMediaQueryCacheCommand command, CancellationToken cancellationToken) =>
        await Result.Try(async Task () =>
        {
            // Phase 1: Warm cache immediately from existing DB data.
            // This ensures returning users see media instantly on container restart.
            _log.Here().Information("Phase 1: Building Media Query Cache from existing database state");
            await _mediaQueryCache.BuildCache();

            // Suppress invalidation during the library sync storm so cache-doom loops
            // are avoided. Ownership/access-triggered invalidations are deferred.
            // Wrap in try/finally so cancellation always resets the flag.
            _mediaQueryCache.SuppressInvalidation = true;
            _log.Here().Debug("Media query cache invalidation suppressed until sync storm settles");

            try
            {
                // Phase 2: Wait for all library sync jobs to quiesce.
                while (true)
                {
                    while (await HasActiveLibrarySyncJobsAsync(cancellationToken))
                        await Task.Delay(_librarySyncPollInterval, cancellationToken);

                    // Re-check after quiet period — sync chains may have queued more jobs.
                    await Task.Delay(SyncQuietPeriod, cancellationToken);

                    if (!await HasActiveLibrarySyncJobsAsync(cancellationToken))
                        break;

                    _log.Here().Debug("New library sync jobs detected after quiet period, waiting again");
                }
            }
            finally
            {
                // Always reset the suppression flag, even on cancellation.
                _mediaQueryCache.SuppressInvalidation = false;
            }

            // Phase 3: Final clean rebuild.
            _log.Here().Information("Phase 3: Rebuilding Media Query Cache after library sync storm settled");
            await _mediaQueryCache.BuildCache();
        });

    private async Task<bool> HasActiveLibrarySyncJobsAsync(CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();
        return await dbContext.LibrarySyncJobQueues.AnyAsync(
            x => x.Status == LibrarySyncJobStatus.Queued || x.Status == LibrarySyncJobStatus.Processing,
            cancellationToken
        );
    }
}