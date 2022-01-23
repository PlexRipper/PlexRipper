using System.Threading.Tasks;
using Logging;
using PlexRipper.Domain;
using PlexRipper.DownloadManager;
using Quartz;

namespace BackgroundServices.DownloadManager.Jobs
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
            await _downloadTracker.StartDownloadClient(downloadTaskId);

            // while (context.CancellationToken.IsCancellationRequested == false)
            // {
            //     // Extension method so we catch TaskCancelled exceptions.
            //     await TaskDelay.Wait(1000, cancellationToken);
            //     Console.WriteLine("keep rollin, rollin, rollin...");
            // }
        }
    }
}