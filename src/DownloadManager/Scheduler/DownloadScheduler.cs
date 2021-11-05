using System;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Domain;
using Quartz;

namespace PlexRipper.DownloadManager
{
    public class DownloadScheduler : IDownloadScheduler
    {
        private readonly IScheduler _scheduler;

        public DownloadScheduler(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task<Result> StartDownloadJob(int downloadTaskId)
        {
            try
            {
                var jobKey = new JobKey($"DownloadTask_{downloadTaskId}", "DownloadJobs");
                if (await _scheduler.CheckExists(jobKey))
                    return Result.Fail($"There is already a download job for downloadTask {downloadTaskId}");

                Log.Debug($"Starting download job for {nameof(DownloadTask)} with id ({downloadTaskId})");
                var job = JobBuilder.Create<DownloadJob>()
                    .UsingJobData(nameof(downloadTaskId), downloadTaskId)
                    .WithIdentity(jobKey)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(new TriggerKey($"DownloadTask_{downloadTaskId}_Trigger", "DownloadTriggers"))
                    .StartNow()
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogFatal();
            }

            return Result.Ok();
        }
    }
}