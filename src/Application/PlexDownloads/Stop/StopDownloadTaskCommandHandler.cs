namespace Reaparr.Application;

public class StopDownloadTaskCommandValidator : AbstractValidator<StopDownloadTaskCommand>
{
    public StopDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StopDownloadTaskCommandHandler : ICommandHandler<StopDownloadTaskCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;
    private readonly IMoveDownloadFileScheduler _moveDownloadFileScheduler;

    public StopDownloadTaskCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        ICommandExecutor commandExecutor,
        IDownloadTaskScheduler downloadTaskScheduler,
        IMoveDownloadFileScheduler moveDownloadFileScheduler
    )
    {
        _log = log.ForContext<StopDownloadTaskCommandHandler>();
        _dbContext = dbContext;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
        _commandExecutor = commandExecutor;
        _downloadTaskScheduler = downloadTaskScheduler;
        _moveDownloadFileScheduler = moveDownloadFileScheduler;
    }

    public async Task<Result> ExecuteAsync(StopDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        var downloadTasks = await _dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);

        // For TvShow/Season, only stop children that are actively running — others may still
        // be queued and must not have their partial files or state disturbed. For Movie/Episode
        // the parent maps 1-to-1 with its file tasks, so always stop regardless of activity.
        var stopOnlyActiveChildren = key.Type is DownloadTaskType.TvShow or DownloadTaskType.Season;

        foreach (var downloadTaskKey in downloadTasks)
        {
            var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
                continue;
            }

            var isDownloading = await _downloadTaskScheduler.IsDownloading(downloadTaskKey, cancellationToken);
            var isMoving = await _moveDownloadFileScheduler.IsDownloadFileMoving(downloadTaskKey, cancellationToken);

            if (stopOnlyActiveChildren && !isDownloading && !isMoving)
            {
                await _dbContext.CreateDownloadClientLog(
                    downloadTaskKey,
                    NotificationLevel.Debug,
                    downloadTask.DownloadStatus,
                    $"Stop requested but skipped because task is not active (status: {downloadTask.DownloadStatus})"
                );
                continue;
            }

            _log.Here().Information("Stopping {DownloadTaskFullTitle}", downloadTask.FullTitle);

            if (isDownloading)
            {
                var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey, cancellationToken);
                if (stopResult.IsCancelled)
                    return stopResult.LogWarning();

                if (stopResult.IsFailed)
                {
                    // At most one download task runs per server; if stopping it fails there is
                    // nothing left to stop safely, so abort.
                    return stopResult.LogError();
                }
            }

            if (isMoving)
            {
                var stopMoveResult = await _moveDownloadFileScheduler.StopMoveDownloadFileJob(
                    downloadTaskKey,
                    cancellationToken
                );
                if (stopMoveResult.IsCancelled)
                    return stopMoveResult.LogWarning();

                if (stopMoveResult.IsFailed)
                    return stopMoveResult.LogError();
            }

            // Only delete the download file when NOT in the completed phase (file is already
            // at its destination directory in that case).
            if (command.DeleteFiles && downloadTask.DownloadTaskPhase != DownloadTaskPhase.Completed)
            {
                _log.Here()
                    .Debug("Deleting partially downloaded files of {DownloadTaskFullTitle}", downloadTask.FullTitle);

                var deleteFilesResult = await _commandExecutor.Send(
                    new DeleteDownloadTaskFilesCommand([downloadTaskKey]),
                    cancellationToken
                );
                if (deleteFilesResult.IsCancelled)
                    return deleteFilesResult.LogWarning();

                if (deleteFilesResult.IsFailed)
                    return deleteFilesResult.LogError();
            }

            _log.Here().Debug($"Resetting download progress for {downloadTaskKey.Id} ({downloadTask.FileName})");

            await _dbContext.ResetDownloadTaskProgress(downloadTaskKey, DownloadStatus.Stopped, cancellationToken);
            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                downloadTaskKey,
                DownloadStatus.Stopped,
                cancellationToken
            );
        }

        return Result.Ok();
    }
}
