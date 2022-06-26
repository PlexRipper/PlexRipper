using BackgroundServices.Jobs;
using PlexRipper.Application;
using Quartz;

namespace BackgroundServices;

public class SchedulerService : ISchedulerService
{
    private readonly IScheduler _scheduler;

    private readonly JobKey _syncServerJobKey = new("SyncServer", "SyncGroup");

    public SchedulerService(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    private async Task<Result> SetupSyncPlexServersJob()
    {
        var job = JobBuilder.Create<SyncServerJob>()
            .WithIdentity(_syncServerJobKey)
            .Build();

        // Trigger the job to run now, and then every 40 seconds
        var trigger = TriggerBuilder.Create()
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(6)
                .RepeatForever())
            .Build();

        await _scheduler.ScheduleJob(job, trigger);

        return Result.Ok();
    }

    public async Task InspectPlexServersAsyncJob(int plexAccountId)
    {
        var job = JobBuilder.Create<InspectPlexServersJob>()
            .WithIdentity($"{nameof(PlexAccount)}_{plexAccountId}", "InspectPlexServersJobs")
            .UsingJobData("plexAccountId", plexAccountId)
            .Build();

        await _scheduler.ScheduleJob(job, TriggerBuilder.Create().StartNow().Build());
    }

    public async Task<Result> TriggerSyncPlexServersJob()
    {
        await _scheduler.TriggerJob(_syncServerJobKey);

        // Max 3 servers at once

        // Each job executes a sync with a plex server

        // Each job will first retrieve movie libraries and then tvShow

        // Progress is stored in the database
        return Result.Ok();
    }

    public async Task<Result> SetupAsync()
    {
        if (!_scheduler.IsStarted)
        {
            Log.Information("Starting Quartz Scheduler");
            await _scheduler.Start();
        }

        await SetupSyncPlexServersJob();

        return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not start Sync Server Scheduler").LogError();
    }

    public async Task<Result> StopAsync(bool graceFully = true)
    {
        if (!_scheduler.IsShutdown)
        {
            await _scheduler.Shutdown(graceFully);
        }

        return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not shutdown Scheduler").LogError();
    }
}