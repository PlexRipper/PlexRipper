using BackgroundServices.Base;
using PlexRipper.Application;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

public class InspectServerScheduler : BaseScheduler, IInspectServerScheduler
{
    private readonly IMediator _mediator;
    protected override JobKey DefaultJobKey => new($"PlexServerId_", nameof(InspectServerScheduler));

    public InspectServerScheduler(IScheduler scheduler, IMediator mediator) : base(scheduler)
    {
        _mediator = mediator;
    }

    public async Task<Result> QueueInspectPlexServerJob(int plexServerId)
    {
        var key = InspectPlexServerJob.GetJobKey(plexServerId);
        if (await IsJobRunning(key))
        {
            return Result.Fail($"A {nameof(InspectPlexServerJob)} with {nameof(plexServerId)} {plexServerId} is already running")
                .LogWarning();
        }

        var job = JobBuilder.Create<InspectPlexServerJob>()
            .UsingJobData(InspectPlexServerJob.PlexServerIdParameter, plexServerId)
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

    public async Task<Result> QueueRefreshAccessiblePlexServersJob(int plexAccountId)
    {
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

    public async Task<Result> QueueInspectPlexServerByPlexAccountIdJob(int plexAccountId)
    {
        var key = InspectPlexServerByPlexAccountIdJob.GetJobKey(plexAccountId);
        if (await IsJobRunning(key))
        {
            return Result.Fail($"A {nameof(InspectPlexServerByPlexAccountIdJob)} with {nameof(plexAccountId)} {plexAccountId} is already running")
                .LogWarning();
        }

        var job = JobBuilder.Create<InspectPlexServerByPlexAccountIdJob>()
            .UsingJobData(InspectPlexServerByPlexAccountIdJob.PlexAccountIdParameter, plexAccountId)
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