using Data.Contracts;
using DownloadManager.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadTaskUpdatedHandler : IRequestHandler<DownloadTaskUpdated>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public DownloadTaskUpdatedHandler(IPlexRipperDbContext dbContext, ISignalRService signalRService)
    {
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Handle(DownloadTaskUpdated notification, CancellationToken cancellationToken)
    {
        var downloadTaskId = notification.Key.Id;
        var plexServerId = notification.Key.PlexServerId;

        var downloadTask = await _dbContext.GetDownloadTaskAsync(notification.Key, cancellationToken);
        if (downloadTask is null)
        {
            ResultExtensions.EntityNotFound(nameof(DownloadTask), downloadTaskId).LogError();
            return;
        }

        var downloadTasks = await _dbContext.GetAllDownloadTasksAsync(plexServerId, cancellationToken);

        // Send away the new result
        await _signalRService.SendDownloadProgressUpdateAsync(plexServerId, downloadTasks, cancellationToken);
    }
}