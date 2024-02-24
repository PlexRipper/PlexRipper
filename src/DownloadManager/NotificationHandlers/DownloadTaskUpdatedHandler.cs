using Data.Contracts;
using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;
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

        await ReCalculateRootDownloadTask(rootDownloadTaskId, cancellationToken);

        // Send away the new result
        var downloadTasks = await _dbContext.PlexServers
            .AsTracking()
            .IncludeDownloadTasks()
            .GetAsync(plexServerId, cancellationToken);
        var rootDownloadTasks = downloadTasks.DownloadTasks.Where(x => x.ParentId == null).ToList();
        await _signalRService.SendDownloadProgressUpdateAsync(plexServerId, rootDownloadTasks, cancellationToken);
    }

    /// <summary>
    ///  Recalculates the root download task based on the children's progress status.
    /// </summary>
    /// <param name="rootDownloadTaskId"> The root download task id. </param>
    /// <param name="cancellationToken"> The cancellation token. </param>
    public async Task ReCalculateRootDownloadTask(int rootDownloadTaskId, CancellationToken cancellationToken)
    {
        var downloadTaskDb = await _dbContext.DownloadTasks
            .IncludeDownloadTasks()
            .GetAsync(rootDownloadTaskId, cancellationToken);

        void SetDownloadStatus(DownloadTask downloadTask)
        {
            if (downloadTask.Children is null || !downloadTask.Children.Any())
                return;

            foreach (var downloadTaskChild in downloadTask.Children)
                SetDownloadStatus(downloadTaskChild);

            downloadTask.DownloadStatus = DownloadTaskActions.Aggregate(downloadTask.Children.Select(x => x.DownloadStatus).ToList());
            downloadTask.DownloadSpeed = downloadTask.Children.Sum(x => x.DownloadSpeed);
            downloadTask.FileTransferSpeed = downloadTask.Children.Sum(x => x.FileTransferSpeed);
            downloadTask.Percentage = (int)downloadTask.Children.Average(x => x.Percentage);
            downloadTask.DataReceived = downloadTask.Children.Sum(x => x.DataReceived);
        }

        SetDownloadStatus(downloadTaskDb);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}