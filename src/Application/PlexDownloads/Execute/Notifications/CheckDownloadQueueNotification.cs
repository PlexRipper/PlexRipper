using Application.Contracts;
using FastEndpoints;

namespace PlexRipper.Application;

public record CheckDownloadQueueNotification : IEvent
{
    public CheckDownloadQueueNotification(int plexServerId)
    {
        PlexServerIds = [plexServerId];
    }

    public CheckDownloadQueueNotification(List<int> plexServerIds)
    {
        PlexServerIds = plexServerIds;
    }

    public List<int> PlexServerIds { get; }
}

public class CheckDownloadQueueHandler : IEventHandler<CheckDownloadQueueNotification>
{
    private readonly IDownloadQueue _downloadQueue;

    public CheckDownloadQueueHandler(IDownloadQueue downloadQueue)
    {
        _downloadQueue = downloadQueue;
    }

    public async Task HandleAsync(CheckDownloadQueueNotification notification, CancellationToken cancellationToken)
    {
        var checkResult = await _downloadQueue.CheckDownloadQueue(notification.PlexServerIds);
        if (checkResult.IsFailed)
            checkResult.LogError();
    }
}
