using System;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Domain;
using Quartz;

namespace BackgroundServices.DownloadManager
{
    public class DownloadScheduler : IDownloadScheduler
    {
        private readonly IScheduler _scheduler;

        public DownloadScheduler(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        private JobKey CreateDownloadJobKey(int downloadTaskId)
        {
            return new JobKey($"DownloadTask_{downloadTaskId}", "DownloadJobs");
        }

        public async Task<Result> StartDownloadJob(int downloadTaskId)
        {
            if (downloadTaskId < 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            try
            {
                var jobKey = CreateDownloadJobKey(downloadTaskId);
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

        public async Task<Result> StopDownloadJob(int downloadTaskId)
        {
            if (downloadTaskId <= 0)
                ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            var isSuccess = await _scheduler.DeleteJob(CreateDownloadJobKey(downloadTaskId));
            return isSuccess ? Result.Ok() : Result.Fail($"Failed to delete {nameof(DownloadJob)} with DownloadTaskId {downloadTaskId}");
        }

        public async Task<Result> SetupAsync()
        {
            return Result.Ok();
        }

        public async Task<Result> StopAsync(bool gracefully = true)
        {
            return Result.Ok();
        }
    }
}