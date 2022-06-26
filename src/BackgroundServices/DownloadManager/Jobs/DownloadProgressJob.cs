using PlexRipper.Application;
using PlexRipper.DownloadManager;
using Quartz;

namespace BackgroundServices.DownloadManager.Jobs
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

            var hashCodeResult = await _downloadProgressNotifier.SendDownloadProgress(plexServerId);
            if (hashCodeResult.IsFailed)
            {
                hashCodeResult.LogError();
            }

            Log.Verbose($"Executed job: {nameof(DownloadProgressJob)} for {nameof(PlexServer)}: {plexServerId} => {hashCodeResult.Value}");

            var trackResult = await _downloadProgressScheduler.TrackDownloadProgress(plexServerId, hashCodeResult.Value);
            if (trackResult.IsFailed)
            {
                trackResult.LogError();
            }
        }
    }
}