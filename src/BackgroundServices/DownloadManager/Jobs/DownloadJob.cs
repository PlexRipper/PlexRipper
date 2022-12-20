using PlexRipper.DownloadManager;
using Quartz;

namespace BackgroundServices.DownloadManager.Jobs;

public class DownloadJob : IJob
{
    private readonly IDownloadTracker _downloadTracker;

    public DownloadJob(IDownloadTracker downloadTracker)
    {
        _downloadTracker = downloadTracker;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var downloadTaskId = dataMap.GetIntValue("downloadTaskId");
        Log.Debug($"Executing job: {nameof(DownloadJob)} for {nameof(DownloadTask)}: {downloadTaskId}");

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            await _downloadTracker.StartDownloadClient(downloadTaskId);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        // while (context.CancellationToken.IsCancellationRequested == false)
        // {
        //     // Extension method so we catch TaskCancelled exceptions.
        //     await TaskDelay.Wait(1000, cancellationToken);
        //     Console.WriteLine("keep rollin, rollin, rollin...");
        // }
    }
}