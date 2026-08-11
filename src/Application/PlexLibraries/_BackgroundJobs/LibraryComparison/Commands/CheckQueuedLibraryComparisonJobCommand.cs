using Reaparr.BackgroundJobs;

namespace Reaparr.Application;

/// <summary>
/// Starts or nudges the singleton library comparison queue worker when persisted queued work exists.
/// </summary>
public record CheckQueuedLibraryComparisonJobCommand : ICommand<Result>;

/// <summary>
/// Validates requests to wake the persisted library comparison queue worker.
/// </summary>
public class CheckQueuedLibraryComparisonJobCommandValidator
    : AbstractValidator<CheckQueuedLibraryComparisonJobCommand>
{
    public CheckQueuedLibraryComparisonJobCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

/// <summary>
/// Schedules or triggers the singleton Quartz worker that drains persisted library comparison queue rows.
/// </summary>
public class CheckQueuedLibraryComparisonJobCommandHandler
    : ICommandHandler<CheckQueuedLibraryComparisonJobCommand, Result>
{
    private const int MAX_ATTEMPTS = 3;
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public CheckQueuedLibraryComparisonJobCommandHandler(ILogger log, IReaparrDbContext dbContext, IScheduler scheduler)
    {
        _log = log.ForContext<CheckQueuedLibraryComparisonJobCommandHandler>();
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(
        CheckQueuedLibraryComparisonJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var jobKey = PlexLibraryComparisonJob.GetJobKey();
        var triggerKey = new TriggerKey($"{jobKey.Name}_trigger", jobKey.Group);
        
        var hasQueuedItems = await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x => x.Status == LibrarySyncJobStatus.Queued, cancellationToken);
        
        var hasProcessingItems = await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x => x.Status == LibrarySyncJobStatus.Processing, cancellationToken);

        if (!hasQueuedItems && !hasProcessingItems)
        {
            _log.Here().Debug("No queued library comparison jobs found");
            return Result.Ok();
        }

        var isAlreadyRunning = (await _scheduler.GetCurrentlyExecutingJobs(cancellationToken))
            .Any(x => x.JobDetail.Key.Equals(jobKey));

        if (isAlreadyRunning)
        {
            _log.Here().Debug("Library comparison queue worker is already running");
            return Result.Ok();
        }

        if (hasProcessingItems)
        {
            var requeuedCount = await RequeueStaleProcessingItemsAsync(cancellationToken);
            hasQueuedItems = hasQueuedItems || requeuedCount > 0;
        }

        if (!hasQueuedItems)
        {
            _log.Here().Debug("No queued library comparison jobs found after processing stale rows");
            return Result.Ok();
        }

        if (await _scheduler.CheckExists(triggerKey, cancellationToken))
        {
            _log.Here().Debug("Library comparison queue worker trigger is already scheduled");
            return Result.Ok();
        }

        var result = await Result.Try(async Task () =>
        {
            var trigger = TriggerBuilder.Create().WithIdentity(triggerKey).ForJob(jobKey).StartNow().Build();

            if (await _scheduler.CheckExists(jobKey, cancellationToken))
                await _scheduler.ScheduleJob(trigger, cancellationToken);
            else
            {
                var job = JobBuilder.Create<PlexLibraryComparisonJob>().WithIdentity(jobKey).StoreDurably().Build();
                await _scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            _log.Here().Debug("Scheduled library comparison queue worker");
        });

        if (result.IsCancelled)
            return result.LogWarning();

        if (result.IsFailed && result.Errors.OfType<ExceptionalError>().Any(x => x.Exception is ObjectAlreadyExistsException))
        {
            _log.Here().Warning("Library comparison queue worker trigger was already scheduled by another caller");
            return Result.Ok();
        }

        result.LogIfFailed();

        return result;
    }

    private async Task<int> RequeueStaleProcessingItemsAsync(CancellationToken cancellationToken)
    {
        var failedCount = await _dbContext.LibraryComparisonJobQueues
            .Where(x => x.Status == LibrarySyncJobStatus.Processing && x.Attempts >= MAX_ATTEMPTS)
            .ExecuteUpdateAsync(
                x => x.SetProperty(y => y.Status, LibrarySyncJobStatus.Failed)
                    .SetProperty(y => y.CompletedAt, DateTime.UtcNow)
                    .SetProperty(y => y.ErrorMessage, "Library comparison exceeded retry attempts while processing"),
                cancellationToken
            );
        var requeuedCount = await _dbContext.LibraryComparisonJobQueues
            .Where(x => x.Status == LibrarySyncJobStatus.Processing && x.Attempts < MAX_ATTEMPTS)
            .ExecuteUpdateAsync(
                x => x.SetProperty(y => y.Status, LibrarySyncJobStatus.Queued)
                    .SetProperty(y => y.StartedAt, (DateTime?)null)
                    .SetProperty(y => y.ErrorMessage, (string?)null),
                cancellationToken
            );

        _log.Here().Warning(
            "Requeued {Count} stale processing library comparison jobs and failed {FailedCount} max-attempt jobs",
            requeuedCount,
            failedCount
        );

        return requeuedCount;
    }
}
