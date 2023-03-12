using Data.Contracts;
using DownloadManager.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadTaskUpdatedHandler : IRequestHandler<DownloadTaskUpdated>
{
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdatedHandler(IMediator mediator, ISignalRService signalRService)
    {
        _mediator = mediator;
        _signalRService = signalRService;
    }

    public async Task Handle(DownloadTaskUpdated notification, CancellationToken cancellationToken)
    {
        var downloadTaskId = notification.DownloadTaskId;
        var plexServerId = notification.PlexServerId;
        var rootDownloadTaskId = notification.RootDownloadTaskId;

        if (notification.GetFromDb)
        {
            var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId), cancellationToken);
            plexServerId = downloadTaskResult.Value.PlexServerId;
            rootDownloadTaskId = downloadTaskResult.Value.RootDownloadTaskId;
        }

        await _mediator.Send(new ReCalculateRootDownloadTaskCommand(rootDownloadTaskId), cancellationToken);

        // Send away the new result
        var downloadTasksResult = await _mediator.Send(new GetDownloadTasksByPlexServerIdQuery(plexServerId), cancellationToken);
        if (downloadTasksResult.IsFailed)
        {
            downloadTasksResult.LogError();
            return;
        }

        await _signalRService.SendDownloadProgressUpdateAsync(plexServerId, downloadTasksResult.Value, cancellationToken);
    }
}