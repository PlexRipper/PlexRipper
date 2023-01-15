using PlexRipper.Application;

namespace PlexRipper.DownloadManager;

public class DownloadTaskFinishedHandler : INotificationHandler<DownloadTaskFinished>
{
    private readonly IDownloadQueue _downloadQueue;
    private readonly IFileMerger _fileMerger;

    public DownloadTaskFinishedHandler(IDownloadQueue downloadQueue, IFileMerger fileMerger)
    {
        _downloadQueue = downloadQueue;
        _fileMerger = fileMerger;
    }

    public async Task Handle(DownloadTaskFinished notification, CancellationToken cancellationToken)
    {
        await _downloadQueue.CheckDownloadQueueServer(notification.PlexServerId);

        var addFileTaskResult = await _fileMerger.AddFileTaskFromDownloadTask(notification.DownloadTaskId);
        if (addFileTaskResult.IsFailed)
            addFileTaskResult.LogError();
    }
}