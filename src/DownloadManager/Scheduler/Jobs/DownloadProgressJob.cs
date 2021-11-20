using System.Threading.Tasks;
using Logging;
using PlexRipper.Domain;
using Quartz;

namespace PlexRipper.DownloadManager
{
    public class DownloadProgressJob : IJob
    {
        private readonly IDownloadProgressNotifier _downloadProgressNotifier;

        public DownloadProgressJob(IDownloadProgressNotifier downloadProgressNotifier)
        {
            _downloadProgressNotifier = downloadProgressNotifier;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var plexServerId = dataMap.GetIntValue("plexServerId");
            Log.Verbose($"Executing job: {nameof(DownloadProgressJob)} for {nameof(PlexServer)}: {plexServerId}");

            await _downloadProgressNotifier.SendDownloadProgress(plexServerId);
        }
    }
}