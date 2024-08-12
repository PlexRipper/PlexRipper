using Data.Contracts;
using FluentValidation;
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
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public QueueInspectPlexServerJobCommandHandler(IPlexRipperDbContext dbContext, IScheduler scheduler)
    {
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> Handle(QueueInspectPlexServerJobCommand command, CancellationToken cancellationToken)
    {
        var plexServerId = command.PlexServerId;

        var plexServer = await _dbContext.PlexServers.GetAsync(plexServerId, cancellationToken);
        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId).LogError();

        if (!plexServer.IsEnabled)
        {
            return ResultExtensions
                .ServerIsNotEnabled(plexServer.Name, plexServerId, nameof(QueueInspectPlexServerJobCommand))
                .LogError();
        }

        var jobKey = InspectPlexServerJob.GetJobKey(plexServerId);
        if (await _scheduler.IsJobRunningAsync(jobKey, cancellationToken))
        {
            return Result
                .Fail($"A {nameof(InspectPlexServerJob)} with {nameof(plexServerId)} {plexServerId} is already running")
                .LogWarning();
        }

        var job = JobBuilder
            .Create<InspectPlexServerJob>()
            .UsingJobData(InspectPlexServerJob.PlexServerIdParameter, plexServerId)
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{jobKey.Name}_trigger", jobKey.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJobAsync(job, trigger, cancellationToken);

        return Result.Ok();
    }
}
