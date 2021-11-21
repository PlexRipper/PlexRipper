using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Domain;
using Quartz;

namespace PlexRipper.DownloadManager
{
    public class DownloadProgressJob : IJob
    {
        private readonly IDownloadProgressNotifier _downloadProgressNotifier;

        private readonly IDownloadProgressScheduler _downloadProgressScheduler;

        public DownloadProgressJob(IDownloadProgressNotifier downloadProgressNotifier, IDownloadProgressScheduler downloadProgressScheduler)
        {
            _downloadProgressNotifier = downloadProgressNotifier;
            _downloadProgressScheduler = downloadProgressScheduler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var plexServerId = dataMap.GetIntValue("plexServerId");
            Log.Verbose($"Executing job: {nameof(DownloadProgressJob)} for {nameof(PlexServer)}: {plexServerId}");

            var hashCodeResult = await _downloadProgressNotifier.SendDownloadProgress(plexServerId);
            if (hashCodeResult.IsFailed)
            {
                hashCodeResult.LogError();
            }

            Log.Debug($"Executing job: {nameof(DownloadProgressJob)} for {nameof(PlexServer)}: {plexServerId} => {hashCodeResult.Value}");

            var trackResult = await _downloadProgressScheduler.TrackDownloadProgress(plexServerId, hashCodeResult.Value);
            if (trackResult.IsFailed)
            {
                trackResult.LogError();
            }
        }
    }
}