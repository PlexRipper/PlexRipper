using Application.Contracts;
using BackgroundServices.Contracts;
using Logging.Interface;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

public class InspectServerScheduler : BaseScheduler, IInspectServerScheduler
{
    private readonly IMediator _mediator;
    protected override JobKey DefaultJobKey => new($"PlexServerId_", nameof(InspectServerScheduler));

    public InspectServerScheduler(ILog log, IScheduler scheduler, IMediator mediator) : base(log, scheduler)
    {
        _mediator = mediator;
    }

    public async Task<Result> QueueRefreshAccessiblePlexServersJob(int plexAccountId)
    {
        if (plexAccountId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexAccountId), plexAccountId);

        var key = RefreshAccessiblePlexServersJob.GetJobKey(plexAccountId);
        if (await IsJobRunning(key))
        {
            return Result.Fail($"A {nameof(RefreshAccessiblePlexServersJob)} with {nameof(plexAccountId)} {plexAccountId} is already running")
                .LogWarning();
        }

        var job = JobBuilder.Create<RefreshAccessiblePlexServersJob>()
            .UsingJobData(RefreshAccessiblePlexServersJob.PlexAccountIdParameter, plexAccountId)
            .WithIdentity(key)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{key.Name}_trigger", key.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        await ScheduleJob(job, trigger);

        return Result.Ok();
    }
}