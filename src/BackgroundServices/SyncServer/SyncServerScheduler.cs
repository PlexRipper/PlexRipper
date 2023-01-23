using BackgroundServices.Contracts;
using Quartz;

namespace BackgroundServices.SyncServer;

public class SyncServerScheduler : BaseScheduler, ISyncServerScheduler
{
    protected override JobKey DefaultJobKey => new($"PlexServerId_", nameof(SyncServerScheduler));
    public SyncServerScheduler(IScheduler scheduler) : base(scheduler) { }

    /// <inheritdoc/>
    public async Task<Result> QueueSyncPlexServerJob(int plexServerId, bool forceSync = false)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId);

        var key = GetJobKey(plexServerId);
        if (await IsJobRunning(key))
        {
            return Result.Fail($"A {nameof(SyncServerJob)} with {nameof(plexServerId)} {plexServerId} is already running")
                .LogWarning();
        }

        var job = JobBuilder.Create<SyncServerJob>()
            .UsingJobData(SyncServerJob.PlexServerIdParameter, plexServerId)
            .UsingJobData(SyncServerJob.ForceSyncParameter, forceSync)
            .WithIdentity(key)
            .Build();

        // Trigger the job to run now
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{key.Name}_trigger", key.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        await ScheduleJob(job, trigger);

        return Result.Ok();
    }
}