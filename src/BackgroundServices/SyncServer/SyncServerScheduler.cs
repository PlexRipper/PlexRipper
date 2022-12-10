using BackgroundServices.Base;
using Quartz;

namespace BackgroundServices.SyncServer;

public class SyncServerScheduler : BaseScheduler, ISyncServerScheduler
{
    protected override JobKey DefaultJobKey => new($"PlexServerId_", nameof(SyncServerScheduler));
    public SyncServerScheduler(IScheduler scheduler) : base(scheduler) { }

    public async Task<Result> QueueSyncPlexServersJob(int plexServerId)
    {
        var key = GetJobKey(plexServerId);
        if (await IsJobRunning(key))
        {
            return Result.Fail($"A {nameof(SyncServerJob)} with {nameof(plexServerId)} {plexServerId} is already running")
                .LogWarning();
        }

        var job = JobBuilder.Create<SyncServerJob>()
            .UsingJobData(SyncServerJob.PlexServerIdParameter, plexServerId)
            .WithIdentity(key)
            .Build();

        // Trigger the job to run now, and then every 40 seconds
        var trigger = TriggerBuilder.Create()
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(6)
                .RepeatForever())
            .Build();

        await ScheduleJob(job, trigger);

        return Result.Ok();
    }
}