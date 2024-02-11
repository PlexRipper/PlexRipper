using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public record QueueInspectPlexServerJobCommand(int PlexServerId) : IRequest<Result>;

public class QueueInspectPlexServerJobCommandValidator : AbstractValidator<QueueInspectPlexServerJobCommand>
{
    public QueueInspectPlexServerJobCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class QueueInspectPlexServerJobCommandHandler : IRequestHandler<QueueInspectPlexServerJobCommand, Result>
{
    private readonly ILog _log;
    private readonly IScheduler _scheduler;
    private readonly IPlexRipperDbContext _dbContext;

    public QueueInspectPlexServerJobCommandHandler(ILog log, IScheduler scheduler, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _scheduler = scheduler;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(QueueInspectPlexServerJobCommand command, CancellationToken cancellationToken)
    {
        var plexServerId = command.PlexServerId;
        var jobKey = InspectPlexServerJob.GetJobKey(plexServerId);
        if (await _scheduler.IsJobRunningAsync(jobKey, cancellationToken))
        {
            return Result.Fail($"A {nameof(InspectPlexServerJob)} with {nameof(plexServerId)} {plexServerId} is already running")
                .LogWarning();
        }

        var job = JobBuilder.Create<InspectPlexServerJob>()
            .UsingJobData(InspectPlexServerJob.PlexServerIdParameter, plexServerId)
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobKey.Name}_trigger", jobKey.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJobAsync(job, trigger, cancellationToken);

        return Result.Ok();
    }
}