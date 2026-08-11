namespace Reaparr.Application;

public class QueueCheckPlexLibraryUpdatesJobCommandValidator : AbstractValidator<QueueCheckPlexLibraryUpdatesJobCommand>
{
    public QueueCheckPlexLibraryUpdatesJobCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class QueueCheckPlexLibraryUpdatesJobCommandHandler
    : ICommandHandler<QueueCheckPlexLibraryUpdatesJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IScheduler _scheduler;

    public QueueCheckPlexLibraryUpdatesJobCommandHandler(ILogger log, IScheduler scheduler)
    {
        _log = log.ForContext<QueueCheckPlexLibraryUpdatesJobCommandHandler>();
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(
        QueueCheckPlexLibraryUpdatesJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var key = CheckPlexLibrariesForUpdatesJob.GetJobKey();

        const int intervalHours = 3;

        if (await _scheduler.CheckExists(key, cancellationToken))
        {
            await _scheduler.DeleteJob(key, cancellationToken);
            _log.Here().Debug("Rescheduling existing CheckPlexLibrariesForUpdatesJob");
        }

        var job = JobBuilder.Create<CheckPlexLibrariesForUpdatesJob>().WithIdentity(key).Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{key.Name}_trigger", key.Group)
            .ForJob(job)
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInHours(intervalHours).RepeatForever())
            .Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        _log.Here()
            .Information("Scheduled automatic Plex library sync check every {IntervalHours} hours", intervalHours);

        return Result.Ok();
    }
}