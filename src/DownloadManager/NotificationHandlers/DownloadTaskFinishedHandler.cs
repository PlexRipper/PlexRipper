using DownloadManager.Contracts;
using FileSystem.Contracts;
using PlexRipper.Application;

namespace PlexRipper.DownloadManager;

public class DownloadTaskFinishedHandler : INotificationHandler<DownloadTaskFinished>
{
    private readonly IDownloadQueue _downloadQueue;
    private readonly IFileMergeScheduler _fileMergeScheduler;

    public DownloadTaskFinishedHandler(IDownloadQueue downloadQueue, IFileMergeScheduler fileMergeScheduler)
    {
        _downloadQueue = downloadQueue;
        _fileMergeScheduler = fileMergeScheduler;
    }

    public async Task Handle(DownloadTaskFinished notification, CancellationToken cancellationToken)
    {
        await _downloadQueue.CheckDownloadQueueServer(notification.PlexServerId);

        var addFileTaskResult = await _fileMergeScheduler.CreateFileTaskFromDownloadTask(notification.DownloadTaskId);
        if (addFileTaskResult.IsFailed)
        {
            addFileTaskResult.LogError();
            return;
        }

        await _fileMergeScheduler.StartFileMergeJob(addFileTaskResult.Value.Id);
    }
}