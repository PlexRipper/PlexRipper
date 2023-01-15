using PlexRipper.Application;

namespace PlexRipper.DownloadManager;

public class CheckDownloadQueueHandler : INotificationHandler<CheckDownloadQueue>
{
    private readonly IDownloadQueue _downloadQueue;

    public CheckDownloadQueueHandler(IDownloadQueue downloadQueue)
    {
        _downloadQueue = downloadQueue;
    }

    public async Task Handle(CheckDownloadQueue notification, CancellationToken cancellationToken)
    {
        await _downloadQueue.CheckDownloadQueue(notification.PlexServerIds);
    }
}