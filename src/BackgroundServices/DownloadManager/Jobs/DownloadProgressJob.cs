using PlexRipper.Application;
using PlexRipper.DownloadManager;
using Quartz;

namespace BackgroundServices.DownloadManager.Jobs;

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
        var dataMap = context.JobDetail.JobDataMap;
        var plexServerId = dataMap.GetIntValue("plexServerId");
        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var hashCodeResult = await _downloadProgressNotifier.SendDownloadProgress(plexServerId);
            if (hashCodeResult.IsFailed)
                hashCodeResult.LogError();

            Log.Verbose($"Executed job: {nameof(DownloadProgressJob)} for {nameof(PlexServer)}: {plexServerId} => {hashCodeResult.Value}");

            var trackResult = await _downloadProgressScheduler.TrackDownloadProgress(plexServerId, hashCodeResult.Value);
            if (trackResult.IsFailed)
                trackResult.LogError();
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }
}