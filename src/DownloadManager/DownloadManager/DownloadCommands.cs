using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using FileSystem.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadCommands : IDownloadCommands
{
    #region Fields

    private readonly IDownloadQueue _downloadQueue;

    private readonly IDirectorySystem _directorySystem;
    private readonly IDownloadTaskValidator _downloadTaskValidator;

    private readonly IMediator _mediator;

    private readonly INotificationsService _notificationsService;

    private readonly IDownloadTaskFactory _downloadTaskFactory;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    #endregion

    #region Constructor

    public DownloadCommands(
        IMediator mediator,
        IDownloadQueue downloadQueue,
        IDirectorySystem directorySystem,
        IDownloadTaskValidator downloadTaskValidator,
        INotificationsService notificationsService,
        IDownloadTaskFactory downloadTaskFactory,
        IDownloadTaskScheduler downloadTaskScheduler)
    {
        _mediator = mediator;
        _downloadQueue = downloadQueue;
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

        Log.Debug($"Successfully added all {validateResult.Value.Count} DownloadTasks");
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

        await _mediator.Send(new UpdateDownloadTasksByIdCommand(regeneratedDownloadTasks.Value));

        var uniquePlexServers = regeneratedDownloadTasks.Value.Select(x => x.PlexServerId).Distinct().ToList();
        await _downloadQueue.CheckDownloadQueue(uniquePlexServers);

        return Result.Ok();
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
            Log.Information(
                $"PlexServer {downloadTasksResult.Value.PlexServer.Name} already has a DownloadTask downloading so another one cannot be started");
            return Result.Ok();
        }

        var nextTask = _downloadQueue.GetNextDownloadTask(new List<DownloadTask> { downloadTasksResult.Value });
        if (nextTask.IsFailed)
            return nextTask.ToResult();

        return await StartDownloadTask(nextTask.Value);
    }

    #region Start

    public async Task<Result> StartDownloadTask(DownloadTask downloadTask)
    {
        if (downloadTask is null)
            return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

        if (downloadTask.IsDownloadable)
            return await _downloadTaskScheduler.StartDownloadTaskJob(downloadTask.Id, downloadTask.PlexServerId);

        return Result.Fail($"Failed to start downloadTask {downloadTask.FullTitle}, it's not directly downloadable.");
    }

    #endregion

    #region Stop

    /// <inheritdoc/>
    public async Task<Result<List<int>>> StopDownloadTasks(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        var stoppedDownloadTaskIds = new List<int>();

        var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
        if (downloadTask.IsFailed)
        {
            await _notificationsService.SendResult(downloadTask);
            return downloadTask.ToResult();
        }

        Log.Information($"Stopping {downloadTask.Value.FullTitle} from downloading");

        // Check if currently downloading
        var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskId);
        if (stopResult.IsFailed)
            await _notificationsService.SendResult(stopResult);

        var removeTempResult = _directorySystem.DeleteAllFilesFromDirectory(downloadTask.Value.DownloadDirectory);
        if (removeTempResult.IsFailed)
            await _notificationsService.SendResult(removeTempResult);

        stoppedDownloadTaskIds.Add(downloadTask.Value.Id);

        var updateResult = await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(stoppedDownloadTaskIds, DownloadStatus.Stopped));
        if (updateResult.IsFailed)
            return updateResult;

        return Result.Ok(stoppedDownloadTaskIds);
    }

    #endregion

    #region Pause

    /// <inheritdoc/>
    public async Task<Result> PauseDownload(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        var failedTasks = new List<int>();
        var errors = new List<IError>();

        var pauseResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskId);
        if (pauseResult.IsFailed)
        {
            failedTasks.Add(downloadTaskId);
            errors.AddRange(pauseResult.Errors);
        }

        return errors.Any()
            ? Result.Ok()
            : Result.Fail($"Failed to Pause {failedTasks.Count} DownloadTasks, Id's: {failedTasks}").WithErrors(errors);
    }

    #endregion

    /// <inheritdoc/>
    public async Task<Result> ClearCompleted(List<int> downloadTaskIds)
    {
        return await _mediator.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds));
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

    #endregion
}