using DownloadManager.Contracts;

namespace PlexRipper.Application;

public record CheckDownloadQueueNotification : INotification
{
    public CheckDownloadQueueNotification(int plexServerId)
    {
        PlexServerIds = new List<int>() { plexServerId };
    }

    public CheckDownloadQueueNotification(List<int> plexServerIds)
    {
        PlexServerIds = plexServerIds;
    }

    public List<int> PlexServerIds { get; }
}

public class CheckDownloadQueueHandler : INotificationHandler<CheckDownloadQueueNotification>
{
    private readonly IDownloadQueue _downloadQueue;

    public CheckDownloadQueueHandler(IDownloadQueue downloadQueue)
    {
        _downloadQueue = downloadQueue;
    }

    public async Task Handle(CheckDownloadQueueNotification notification, CancellationToken cancellationToken)
    {
        await _downloadQueue.CheckDownloadQueue(notification.PlexServerIds);
    }
}