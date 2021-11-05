using System.Threading.Tasks;
using Logging;
using PlexRipper.Domain;
using Quartz;

namespace PlexRipper.DownloadManager
{
    public class DownloadJob : IJob
    {
        private readonly IDownloadTracker _downloadTracker;


        public DownloadJob(IDownloadTracker downloadTracker)
        {
            _downloadTracker = downloadTracker;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var downloadTaskId = dataMap.GetIntValue("downloadTaskId");
            Log.Debug($"Executing job: {nameof(DownloadJob)} for {nameof(DownloadTask)}: {downloadTaskId}");
            await _downloadTracker.ExecuteDownloadClient(downloadTaskId);
        }
    }
}