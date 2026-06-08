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
    private static readonly TimeSpan _librarySyncQuietPeriod = TimeSpan.FromSeconds(5);

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

    public async Task<Result> ExecuteAsync(WarmupMediaQueryCacheCommand command, CancellationToken cancellationToken) =>
        await Result.Try(async Task () =>
        {
            while (true)
            {
                while (await HasActiveLibrarySyncJobsAsync(cancellationToken))
                    await Task.Delay(_librarySyncPollInterval, cancellationToken);

                await Task.Delay(_librarySyncQuietPeriod, cancellationToken);
                break;
            }

            _log.Here().Information("Building Media Query Cache from warmup");
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