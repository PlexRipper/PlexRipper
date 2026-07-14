using Quartz;

namespace Reaparr.BackgroundJobs;

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
        var hasQueuedItems = await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x => x.Status == LibrarySyncJobStatus.Queued, cancellationToken);

        if (!hasQueuedItems)
        {
            _log.Here().Debug("No queued library comparison jobs found");
            return Result.Ok();
        }

        var jobKey = PlexLibraryComparisonJob.GetJobKey();
        if (await _scheduler.CheckExists(jobKey, cancellationToken))
        {
            await _scheduler.TriggerJob(jobKey, cancellationToken);
            return Result.Ok();
        }

        var job = JobBuilder.Create<PlexLibraryComparisonJob>().WithIdentity(jobKey).Build();

        var trigger = TriggerBuilder.Create().WithIdentity($"{jobKey.Name}_trigger", jobKey.Group).StartNow().Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);

        _log.Here().Debug("Scheduled library comparison queue worker");

        return Result.Ok();
    }
}