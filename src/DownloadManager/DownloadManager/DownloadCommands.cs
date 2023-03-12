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
    public async Task<Result> CreateDownloadTasks(List<DownloadTask> downloadTasks)
    {
        if (!downloadTasks.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

        var validateResult = _downloadTaskValidator.ValidateDownloadTasks(downloadTasks);
        if (validateResult.IsFailed)
            return validateResult.ToResult().LogDebug();

        // Add to Database
        var createResult = await _mediator.Send(new CreateDownloadTasksCommand(validateResult.Value));
        if (createResult.IsFailed)
            return createResult.ToResult().LogError();

        _log.Debug("Successfully added all {ValidateCount} DownloadTasks", validateResult.Value.Count);
        var uniquePlexServers = downloadTasks.Select(x => x.PlexServerId).Distinct().ToList();

        await _mediator.Publish(new CheckDownloadQueue(uniquePlexServers));
        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result> RestartDownloadTask(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        var stopDownloadTasksResult = await StopDownloadTasks(downloadTaskId);
        if (stopDownloadTasksResult.IsFailed)
            return stopDownloadTasksResult.ToResult().LogError();

        var regeneratedDownloadTasks = await _downloadTaskFactory.RegenerateDownloadTask(stopDownloadTasksResult.Value);
        if (regeneratedDownloadTasks.IsFailed)
            return regeneratedDownloadTasks.LogError();

        var downloadTasks = regeneratedDownloadTasks.Value;
        var restartedDownloadTask = downloadTasks.Flatten(x => x.Children).FirstOrDefault(x => x.Id == downloadTaskId);
        if (restartedDownloadTask == null)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogError();

        await _mediator.Send(new UpdateDownloadTasksByIdCommand(downloadTasks));

        await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(downloadTaskId, DownloadStatus.Queued));

        return await SetDownloadTaskUpdated(downloadTaskId);
    }

    public async Task<Result> ResumeDownloadTask(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        var downloadTasksResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true));
        if (downloadTasksResult.IsFailed)
            return downloadTasksResult.ToResult();

        if (await _downloadTaskScheduler.IsDownloading(downloadTaskId))
        {
            _log.Information("PlexServer {PlexServerName} already has a DownloadTask downloading so another one cannot be started",
                downloadTasksResult.Value.PlexServer.Name);
            return Result.Ok();
        }

        await _mediator.Publish(new CheckDownloadQueue(downloadTasksResult.Value.PlexServerId));

        return Result.Ok();
    }

    #region Start

    public async Task<Result> StartDownloadTask(DownloadTask downloadTask)
    {
        if (downloadTask is null)
            return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

        if (downloadTask.IsDownloadable)
            return await _downloadTaskScheduler.StartDownloadTaskJob(downloadTask.Id, downloadTask.PlexServerId);

        await _mediator.Publish(new CheckDownloadQueue(downloadTask.PlexServerId));

        return Result.Fail($"Failed to start downloadTask {downloadTask.FullTitle}, it's not directly downloadable.");
    }

    public async Task<Result> StartDownloadTask(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));

        if (downloadTaskResult.IsFailed)
            return downloadTaskResult.ToResult();

        return await StartDownloadTask(downloadTaskResult.Value);
    }

    #endregion

    #region Stop

    /// <inheritdoc/>
    public async Task<Result<List<int>>> StopDownloadTasks(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
        if (downloadTask.IsFailed)
        {
            await _notificationsService.SendResult(downloadTask);
            return downloadTask.ToResult();
        }

        _log.Information("Stopping {DownloadTaskFullTitle} from downloading", downloadTask.Value.FullTitle);

        var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskId);
        if (stopResult.IsFailed)
            await _notificationsService.SendResult(stopResult);

        _log.Debug("Deleting partially downloaded files of {DownloadTaskFullTitle}", downloadTask.Value.FullTitle);

        var removeTempResult = _directorySystem.DeleteAllFilesFromDirectory(downloadTask.Value.DownloadDirectory);
        if (removeTempResult.IsFailed)
            await _notificationsService.SendResult(removeTempResult);

        return await SetDownloadTaskUpdated(downloadTaskId);
    }

    #endregion

    #region Pause

    /// <inheritdoc/>
    public async Task<Result> PauseDownloadTask(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        _log.Information("Pausing DownloadTask with id {DownloadTaskFullTitle} from downloading", downloadTaskId);

        var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskId);
        if (stopResult.IsFailed)
            await _notificationsService.SendResult(stopResult);

        await _downloadTaskScheduler.AwaitDownloadTaskJob(downloadTaskId);

        _log.Debug("DownloadTask {DownloadTaskId} has been Paused, meaning no downloaded files have been deleted", downloadTaskId);

        var updateResult = await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(downloadTaskId, DownloadStatus.Paused));
        if (updateResult.IsFailed)
            return updateResult.LogError();

        return await SetDownloadTaskUpdated(downloadTaskId);
    }

    #endregion

    /// <inheritdoc/>
    public Task<Result> ClearCompleted(List<int> downloadTaskIds)
    {
        return _mediator.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds));
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> DeleteDownloadTaskClients(List<int> downloadTaskIds)
    {
        if (downloadTaskIds is null || !downloadTaskIds.Any())
            return Result.Fail("Parameter downloadTaskIds was empty or null").LogError();

        foreach (var downloadTaskId in downloadTaskIds)
            if (await _downloadTaskScheduler.IsDownloading(downloadTaskId))
                await StopDownloadTasks(downloadTaskId);

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