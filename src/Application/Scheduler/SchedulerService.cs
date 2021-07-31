using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;
using Quartz;

namespace PlexRipper.Application
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IScheduler _scheduler;

        private readonly JobKey _syncServerJobKey = new("SyncServer", "SyncGroup");

        private readonly TriggerKey _syncServerTriggerKey = new("StartNow", "TriggerGroup");

        public SchedulerService(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        private async Task<Result> SetupSyncPlexServersJob()
        {
            IJobDetail job = JobBuilder.Create<SyncServerJob>()
                .WithIdentity(_syncServerJobKey)
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(_syncServerTriggerKey)
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(6)
                    .RepeatForever())
                .Build();

            await _scheduler.ScheduleJob(job, trigger);

            return Result.Ok();
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
            await _scheduler.Start();

            await SetupSyncPlexServersJob();

            return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not start Sync Server Scheduler").LogError();
        }
    }
}