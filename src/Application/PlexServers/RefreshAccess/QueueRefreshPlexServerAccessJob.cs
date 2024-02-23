using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

/// <summary>
/// Queue a job to retrieves the latest accessible <see cref="PlexServer">PlexServers</see> for this <see cref="PlexAccount"/> from the PlexAPI and stores it in the Database.
/// </summary>
/// <param name="PlexAccountId">The id of the <see cref="PlexAccount"/> to check.</param>
public record QueueRefreshPlexServerAccessJob(int PlexAccountId) : IRequest<Result>;

public class QueueRefreshPlexServerAccessJobValidator : AbstractValidator<QueueRefreshPlexServerAccessJob>
{
    public QueueRefreshPlexServerAccessJobValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class QueueRefreshPlexServerAccessJobHandler : IRequestHandler<QueueRefreshPlexServerAccessJob, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public QueueRefreshPlexServerAccessJobHandler(ILog log, IPlexRipperDbContext dbContext, IScheduler scheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> Handle(QueueRefreshPlexServerAccessJob command, CancellationToken cancellationToken)
    {
        var plexAccountId = command.PlexAccountId;
        var key = RefreshPlexServersAccessJob.GetJobKey(plexAccountId);
        if (await _scheduler.IsJobRunningAsync(key, cancellationToken))
        {
            return Result.Fail($"A {nameof(RefreshPlexServersAccessJob)} with {nameof(plexAccountId)} {plexAccountId} is already running")
                .LogWarning();
        }

        var job = JobBuilder.Create<RefreshPlexServersAccessJob>()
            .UsingJobData(RefreshPlexServersAccessJob.PlexAccountIdParameter, plexAccountId)
            .WithIdentity(key)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{key.Name}_trigger", key.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);

        return Result.Ok();
    }
}