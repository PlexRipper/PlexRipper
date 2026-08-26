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
        await Result.Try(async Task<Result> () =>
        {
            _log.Here().Debug("Media query cache invalidation remains suppressed until sync storm settles");

            try
            {
                // Wait for all library sync jobs to quiesce.
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

                _log.Here().Information("Rebuilding Media Query Cache after library sync storm settled");
                return await _mediaQueryCache.BuildCache(cancellationToken);
            }
            finally
            {
                // Always reset the suppression flag after the final build, even on failure or cancellation.
                _mediaQueryCache.SuppressInvalidation = false;
            }
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
