using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using Quartz;

namespace PlexRipper.DownloadManager
{
    public class DownloadProgressScheduler : IDownloadProgressScheduler
    {
        #region Fields

        private readonly string _downloadProgressKey = "ServerDownloadProgress";

        private readonly IScheduler _scheduler;

        #endregion

        #region Constructor

        public DownloadProgressScheduler(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        #endregion

        #region Public Methods

        public async Task<Result> StartDownloadProgressJob(int plexServerId)
        {
            if (plexServerId <= 0)
                return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

            var jobKey = CreateDownloadProgressJobKey(plexServerId);
            if (await _scheduler.CheckExists(jobKey))
            {
                return Result.Fail($"Job with {jobKey} already exists");
            }

            var job = JobBuilder.Create<DownloadProgressJob>()
                .UsingJobData(nameof(plexServerId), plexServerId)
                .WithIdentity(jobKey)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(new TriggerKey($"{_downloadProgressKey}_{plexServerId}_Trigger", "DownloadTriggers"))
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)
                    .RepeatForever())
                .StartNow()
                .Build();

            await _scheduler.ScheduleJob(job, trigger);

            return Result.Ok();
        }

        public async Task<Result> StopDownloadProgressJob(int plexServerId)
        {
            if (plexServerId <= 0)
                ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

            var isSuccess = await _scheduler.DeleteJob(CreateDownloadProgressJobKey(plexServerId));
            return isSuccess ? Result.Ok() : Result.Fail($"Failed to delete {nameof(DownloadProgressJob)} with PlexServerId {plexServerId}");
        }

        #endregion

        #region Private Methods

        private JobKey CreateDownloadProgressJobKey(int plexServerId)
        {
            return new JobKey($"{_downloadProgressKey}_{plexServerId}", "DownloadProgressJobs");
        }

        #endregion
    }
}