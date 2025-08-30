using Application.Contracts;
using FastEndpoints;

namespace PlexRipper.Application;

public record CheckDownloadQueueEvent : IEvent
{
    public CheckDownloadQueueEvent(int plexServerId)
    {
        PlexServerIds = [plexServerId];
    }

    public CheckDownloadQueueEvent(List<int> plexServerIds)
    {
        PlexServerIds = plexServerIds;
    }

    public List<int> PlexServerIds { get; }
}

public class CheckDownloadQueueHandler : IEventHandler<CheckDownloadQueueEvent>
{
    private readonly IDownloadQueue _downloadQueue;

    public CheckDownloadQueueHandler(IDownloadQueue downloadQueue)
    {
        _downloadQueue = downloadQueue;
    }

    public async Task HandleAsync(CheckDownloadQueueEvent @event, CancellationToken cancellationToken)
    {
        var checkResult = await _downloadQueue.CheckDownloadQueue(@event.PlexServerIds);
        if (checkResult.IsFailed)
            checkResult.LogError();
    }
}
