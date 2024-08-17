using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public record QueueSyncServerMediaJobCommand(int PlexServerId, bool ForceSync = false) : IRequest<Result>;

public class QueueSyncServerMediaJobCommandValidator : Validator<QueueSyncServerMediaJobCommand>
{
    public QueueSyncServerMediaJobCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class QueueSyncServerMediaJobCommandHandler : IRequestHandler<QueueSyncServerMediaJobCommand, Result>
{
    private readonly ILog _log;
    private readonly IScheduler _scheduler;

    public QueueSyncServerMediaJobCommandHandler(ILog log, IScheduler scheduler)
    {
        _log = log;
        _scheduler = scheduler;
    }

    public async Task<Result> Handle(QueueSyncServerMediaJobCommand command, CancellationToken cancellationToken)
    {
        if (command.PlexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(command.PlexServerId), command.PlexServerId);

        var key = SyncServerMediaJob.GetJobKey(command.PlexServerId);
        if (await _scheduler.IsJobRunningAsync(key, cancellationToken))
        {
            return Result
                .Fail(
                    $"A {nameof(SyncServerMediaJob)} with {nameof(PlexServer)} {command.PlexServerId} is already running"
                )
                .LogWarning();
        }

        var job = JobBuilder
            .Create<SyncServerMediaJob>()
            .UsingJobData(SyncServerMediaJob.PlexServerIdParameter, command.PlexServerId)
            .UsingJobData(SyncServerMediaJob.ForceSyncParameter, command.ForceSync)
            .WithIdentity(key)
            .Build();

        // Trigger the job to run now
        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{key.Name}_trigger", key.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        _log.Information("Sync Server Job for server with id {PlexServerId} has started", command.PlexServerId);
        await _scheduler.ScheduleJob(job, trigger, cancellationToken);

        return Result.Ok();
    }
}
