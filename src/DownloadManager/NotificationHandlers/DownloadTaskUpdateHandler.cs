using DownloadManager.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadTaskUpdateHandler : INotificationHandler<DownloadTaskUpdated>
{
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdateHandler(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task Handle(DownloadTaskUpdated notification, CancellationToken cancellationToken)
    {
        await _signalRService.SendDownloadTaskUpdateAsync(notification.DownloadTask, cancellationToken);
    }
}