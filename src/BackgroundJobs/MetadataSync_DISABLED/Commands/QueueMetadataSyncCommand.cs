using FastEndpoints;
using FluentValidation;
using Quartz;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Command to queue a metadata sync job for a specific Plex server.
/// </summary>
/// <param name="ServerId">The ID of the Plex server to sync metadata for.</param>
public record QueueMetadataSyncCommand(int ServerId) : ICommand<Result>;

public class QueueMetadataSyncCommandValidator : AbstractValidator<QueueMetadataSyncCommand>
{
    public QueueMetadataSyncCommandValidator()
    {
        RuleFor(x => x.ServerId).GreaterThan(0);
    }
}

public class QueueMetadataSyncCommandHandler : ICommandHandler<QueueMetadataSyncCommand, Result>
{
    private readonly IScheduler _scheduler;
    private readonly ILogger _log;

    public QueueMetadataSyncCommandHandler(IScheduler scheduler, ILogger log)
    {
        _scheduler = scheduler;
        _log = log.ForContext<QueueMetadataSyncCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(QueueMetadataSyncCommand command, CancellationToken ct)
    {
        var jobKey = MetadataSyncJob.GetJobKey(command.ServerId);

        // Don't queue if already running/scheduled
        if (await _scheduler.CheckExists(jobKey, ct))
        {
            _log.Here().Debug("MetadataSyncJob already scheduled for server {ServerId}", command.ServerId);
            return Result.Ok();
        }

        var job = JobBuilder
            .Create<MetadataSyncJob>()
            .WithIdentity(jobKey)
            .UsingJobData(MetadataSyncJob.ServerIdParameter, command.ServerId)
            .Build();

        var trigger = TriggerBuilder.Create().WithIdentity($"{jobKey.Name}_trigger", jobKey.Group).StartNow().Build();

        await _scheduler.ScheduleJob(job, trigger, ct);

        _log.Here().Information("Scheduled MetadataSyncJob for server {ServerId}", command.ServerId);

        return Result.Ok();
    }
}
