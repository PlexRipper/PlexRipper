using Data.Contracts;
using DownloadManager.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadTaskUpdatedHandler : IRequestHandler<DownloadTaskUpdated>
{
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdatedHandler(IMediator mediator, IPlexRipperDbContext dbContext, ISignalRService signalRService)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Handle(DownloadTaskUpdated notification, CancellationToken cancellationToken)
    {
        var downloadTaskId = notification.DownloadTaskId;
        var plexServerId = notification.PlexServerId;
        var rootDownloadTaskId = notification.RootDownloadTaskId;

        if (notification.GetFromDb)
        {
            var downloadTask = await _dbContext.DownloadTasks.GetAsync(downloadTaskId, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTask), downloadTaskId).LogError();
                return;
            }
            plexServerId = downloadTask.PlexServerId;
            rootDownloadTaskId = downloadTask.RootDownloadTaskId;
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