using Data.Contracts;
using DownloadManager.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadTaskUpdateHandler : IRequestHandler<DownloadTaskUpdated>
{
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdateHandler(IMediator mediator, ISignalRService signalRService)
    {
        _mediator = mediator;
        _signalRService = signalRService;
    }

    public async Task Handle(DownloadTaskUpdated notification, CancellationToken cancellationToken)
    {
        var downloadTaskId = notification.DownloadTaskId;
        var plexServerId = notification.PlexServerId;
        var rootDownloadTaskId = notification.RootDownloadTaskId;

        await _mediator.Send(new ReCalculateRootDownloadTaskCommand(rootDownloadTaskId), cancellationToken);

        // Send away the new result
        var downloadTasksResult = await _mediator.Send(new GetDownloadTasksByPlexServerIdQuery(plexServerId), cancellationToken);
        if (downloadTasksResult.IsFailed)
        {
            downloadTasksResult.LogError();
            return;
        }

        await _signalRService.SendDownloadProgressUpdateAsync(plexServerId, downloadTasksResult.Value);

        // if (true)
        // {
        //     // TODO SignalR endpoint can maybe be removed
        //     var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true), cancellationToken);
        //     await _signalRService.SendDownloadTaskUpdateAsync(downloadTaskResult.Value, cancellationToken);
        // }
    }
}