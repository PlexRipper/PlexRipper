using Data.Contracts;
using DownloadManager.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadStatusChangedHandler : INotificationHandler<DownloadStatusChanged>
{
    private readonly IMediator _mediator;

    public DownloadStatusChangedHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(DownloadStatusChanged notification, CancellationToken cancellationToken)
    {
        var listIds = new List<int>
        {
            notification.DownloadTaskId,
        };

        // TODO Can maybe be avoided due to the PlexRipperClient also updating the progress with status
        await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(listIds, notification.Status), cancellationToken);

        await _mediator.Send(new UpdateRootDownloadStatusOfDownloadTaskCommand(notification.RootDownloadTaskId), cancellationToken);
    }
}