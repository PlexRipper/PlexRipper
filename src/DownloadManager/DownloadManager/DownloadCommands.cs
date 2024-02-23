using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using FileSystem.Contracts;
using Logging.Interface;

namespace PlexRipper.DownloadManager;

public class DownloadCommands : IDownloadCommands
{
    #region Fields

    private readonly IDirectorySystem _directorySystem;
    private readonly IDownloadTaskValidator _downloadTaskValidator;

    private readonly ILog _log;
    private readonly IMediator _mediator;

    private readonly INotificationsService _notificationsService;

    private readonly IDownloadTaskFactory _downloadTaskFactory;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    #endregion

    #region Constructor

    public DownloadCommands(
        ILog log,
        IMediator mediator,
        IDirectorySystem directorySystem,
        IDownloadTaskValidator downloadTaskValidator,
        INotificationsService notificationsService,
        IDownloadTaskFactory downloadTaskFactory,
        IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _mediator = mediator;
        _directorySystem = directorySystem;
        _downloadTaskValidator = downloadTaskValidator;
        _notificationsService = notificationsService;
        _downloadTaskFactory = downloadTaskFactory;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteDownloadTaskClients(List<int> downloadTaskIds)
    {
        if (downloadTaskIds is null || !downloadTaskIds.Any())
            return Result.Fail("Parameter downloadTaskIds was empty or null").LogError();

        // foreach (var downloadTaskId in downloadTaskIds)
        //     if (await _downloadTaskScheduler.IsDownloading(downloadTaskId))
        //         await StopDownloadTasks(downloadTaskId);

        return await _mediator.Send(new DeleteDownloadTasksByIdCommand(downloadTaskIds));
    }

    private async Task<Result> SetDownloadTaskUpdated(int downloadTaskId)
    {
        var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
        if (downloadTask.IsFailed)
        {
            await _notificationsService.SendResult(downloadTask);
            return downloadTask.ToResult();
        }

        await _mediator.Send(new DownloadTaskUpdated(downloadTask.Value));

        await _mediator.Publish(new CheckDownloadQueue(downloadTask.Value.PlexServerId));

        return Result.Ok();
    }

    #endregion
}