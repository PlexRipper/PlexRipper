using Quartz;

namespace Reaparr.BackgroundJobs;

public class QueueLibraryMediaCompareJobCommandValidator : AbstractValidator<QueueLibraryMediaCompareJobCommand>
{
    public QueueLibraryMediaCompareJobCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.RemotePlexLibraryId).GreaterThan(0);
        RuleFor(x => x.OwnedPlexLibraryId).GreaterThan(0);
        RuleFor(x => x.MediaType).IsInEnum();
    }
}

public class QueueLibraryMediaCompareJobCommandHandler : ICommandHandler<QueueLibraryMediaCompareJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IScheduler _scheduler;

    public QueueLibraryMediaCompareJobCommandHandler(ILogger log, IScheduler scheduler)
    {
        _log = log.ForContext<QueueLibraryMediaCompareJobCommandHandler>();
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(
        QueueLibraryMediaCompareJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var (remoteLibraryId, ownedLibraryId, mediaType) = command;

        var jobKey = PlexLibraryComparisonJob.GetJobKey(remoteLibraryId, ownedLibraryId, mediaType);

        // Replace existing job if one already exists for this pair
        if (await _scheduler.CheckExists(jobKey, cancellationToken))
        {
            await _scheduler.DeleteJob(jobKey, cancellationToken);
        }

        var jobDataMap = new JobDataMap
        {
            [PlexLibraryComparisonJob.RemoteLibraryIdParameter] = remoteLibraryId,
            [PlexLibraryComparisonJob.OwnedLibraryIdParameter] = ownedLibraryId,
            [PlexLibraryComparisonJob.MediaTypeParameter] = (int)mediaType,
        };

        var job = JobBuilder.Create<PlexLibraryComparisonJob>()
            .WithIdentity(jobKey)
            .SetJobData(jobDataMap)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobKey.Name}_trigger", jobKey.Group)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);

        _log.Here()
            .Debug(
                "Scheduled library comparison job: remote {RemoteLibId} vs owned {OwnedLibId}, {MediaType}",
                remoteLibraryId,
                ownedLibraryId,
                mediaType
            );

        return Result.Ok();
    }
}
