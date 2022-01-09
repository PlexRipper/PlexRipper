using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Domain;
using Quartz;

namespace BackgroundServices.DownloadManager
{
    public class DownloadProgressScheduler : IDownloadProgressScheduler
    {
        #region Fields

        private readonly string _downloadProgressKey = "ServerDownloadProgress";

        private readonly IScheduler _scheduler;

        private readonly Dictionary<int, List<string>> _trackDictionary = new();

        private readonly int _numberOfSameUpdates = 20;

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
                return Result.Fail($"Job with {jobKey} already exists").LogWarning();
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

        public async Task<Result> FireDownloadProgressJob(int plexServerId)
        {
            try
            {
                var jobKey = CreateDownloadProgressJobKey(plexServerId);
                var jobData = new JobDataMap();
                jobData.Put(nameof(plexServerId), plexServerId);
                await _scheduler.TriggerJob(jobKey, jobData);
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
        }

        public async Task<Result> StopDownloadProgressJob(int plexServerId)
        {
            if (plexServerId <= 0)
                ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

            var isSuccess = await _scheduler.DeleteJob(CreateDownloadProgressJobKey(plexServerId));
            if (isSuccess)
            {
                _trackDictionary.Remove(plexServerId);
                Log.Information($"{nameof(DownloadProgressJob)} for {nameof(PlexServer)} {plexServerId} was stopped");
                return Result.Ok();
            }

            return Result.Fail($"Failed to delete {nameof(DownloadProgressJob)} with PlexServerId {plexServerId}");
        }

        public async Task<Result> TrackDownloadProgress(int plexServerId, string hashCode)
        {
            if (plexServerId <= 0)
                ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

            if (string.IsNullOrEmpty(hashCode))
                return ResultExtensions.IsEmpty(nameof(hashCode)).LogWarning();

            if (!_trackDictionary.ContainsKey(plexServerId))
            {
                _trackDictionary.Add(plexServerId, new List<string> { hashCode });
            }

            _trackDictionary[plexServerId].Add(hashCode);

            if (_trackDictionary[plexServerId].Count > _numberOfSameUpdates)
            {
                _trackDictionary[plexServerId].RemoveAt(0);
            }

            if (_trackDictionary[plexServerId].Count >= _numberOfSameUpdates && _trackDictionary[plexServerId].All(x => x == hashCode))
            {
                Log.Debug($"Download progress job has been sending out the same {_numberOfSameUpdates} updates, will stop now.");
                await StopDownloadProgressJob(plexServerId);
            }

            return Result.Ok();
        }

        #endregion

        #region Private Methods

        private JobKey CreateDownloadProgressJobKey(int plexServerId)
        {
            return new JobKey($"{_downloadProgressKey}_{plexServerId}", "DownloadProgressJobs");
        }

        #endregion

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