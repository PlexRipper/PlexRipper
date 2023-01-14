using PlexRipper.Application;
using PlexRipper.Application.BackgroundServices;
using Quartz;

namespace BackgroundServices.InspectPlexServer;

public class InspectServerScheduler : BaseScheduler, IInspectServerScheduler
{
    private readonly IPlexServerService _plexServerService;
    protected override JobKey DefaultJobKey => new($"PlexServerId_", nameof(InspectServerScheduler));

    public InspectServerScheduler(IScheduler scheduler, IPlexServerService plexServerService) : base(scheduler)
    {
        _plexServerService = plexServerService;
    }

    public async Task<Result> QueueInspectPlexServerJob(int plexServerId)
    {
        var jobKey = InspectPlexServerJob.GetJobKey(plexServerId);
        if (await IsJobRunning(jobKey))
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

        await ScheduleJob(job, trigger);

        return Result.Ok();
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

    public async Task<Result> QueueInspectPlexServerByPlexAccountIdJob(int plexAccountId)
    {
        if (plexAccountId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexAccountId), plexAccountId);

        var key = InspectPlexServerByPlexAccountIdJob.GetJobKey(plexAccountId);
        if (await IsJobRunning(key))
        {
            return Result.Fail($"A {nameof(InspectPlexServerByPlexAccountIdJob)} with {nameof(plexAccountId)} {plexAccountId} is already running")
                .LogWarning();
        }

        // Before returning we must ensure the accessible plex servers are retrieved because otherwise the front-end will prematurely re-fetch the created plex account without any accessibility.
        var plexServerResult = await _plexServerService.RefreshAccessiblePlexServersAsync(plexAccountId);
        if (plexServerResult.IsFailed)
            return plexServerResult.ToResult();

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