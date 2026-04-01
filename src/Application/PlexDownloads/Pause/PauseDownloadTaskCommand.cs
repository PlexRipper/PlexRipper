using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Pauses and disposes of the PlexDownloadClient executing the <see cref="DownloadTaskGeneric"/> if it is downloading or pauses the FileTransfer
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to Pause.</param>
/// <returns>If successful a list of the DownloadTasks that were Paused.</returns>
public record PauseDownloadTaskCommand(Guid DownloadTaskGuid) : ICommand<Result>;

public class PauseDownloadTaskCommandValidator : AbstractValidator<PauseDownloadTaskCommand>
{
    public PauseDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class PauseDownloadTaskCommandHandler : ICommandHandler<PauseDownloadTaskCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;
    private readonly IMoveDownloadFileScheduler _moveDownloadFileScheduler;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;

    public PauseDownloadTaskCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IDownloadTaskScheduler downloadTaskScheduler,
        IMoveDownloadFileScheduler moveDownloadFileScheduler,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher
    )
    {
        _log = log.ForContext<PauseDownloadTaskCommandHandler>();
        _dbContext = dbContext;
        _downloadTaskScheduler = downloadTaskScheduler;
        _moveDownloadFileScheduler = moveDownloadFileScheduler;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
    }

    public async Task<Result> ExecuteAsync(PauseDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        var downloadTasks = await _dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);
        foreach (var downloadTaskKey in downloadTasks)
        {
            var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
                continue;
            }

            if (downloadTask.DownloadTaskPhase == DownloadTaskPhase.Completed)
                continue;

            // A finished move must never be reset — the file is already at the destination.
            // MoveDownloadFileJob will transition it to Completed; pausing here would corrupt progress.
            if (downloadTask.DownloadStatus == DownloadStatus.MoveFinished)
                continue;

            // DownloadFinished means the file is ready to move but the move job has not started yet.
            // It still needs to be marked paused so it is not picked up as runnable.
            if (downloadTask.DownloadStatus == DownloadStatus.DownloadFinished)
            {
                var queuedPauseResult = await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    downloadTaskKey,
                    DownloadStatus.Paused,
                    cancellationToken
                );
                if (queuedPauseResult.IsFailed)
                    return queuedPauseResult.LogError();
                continue;
            }

            _log.Here()
                .Information("Pausing DownloadTask with id {DownloadTaskTitle} from downloading", downloadTask.Title);

            if (downloadTask.DownloadTaskPhase == DownloadTaskPhase.FileTransfer)
            {
                var isMoving = await _moveDownloadFileScheduler.IsDownloadFileMoving(downloadTaskKey);
                if (isMoving)
                {
                    var stopMoveResult = await _moveDownloadFileScheduler.StopMoveDownloadFileJob(downloadTaskKey);
                    if (stopMoveResult.IsFailed)
                        return stopMoveResult.LogError();
                }

                var resetMoveProgressResult = await _dbContext.ResetDownloadTaskProgress(
                    downloadTaskKey,
                    DownloadStatus.MovePaused,
                    cancellationToken
                );
                if (resetMoveProgressResult.IsFailed)
                    return resetMoveProgressResult.LogError();
                continue;
            }

            var isDownloading = await _downloadTaskScheduler.IsDownloading(downloadTaskKey, cancellationToken);
            if (!isDownloading)
            {
                var inactivePauseResult = await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    downloadTaskKey,
                    DownloadStatus.Paused,
                    cancellationToken
                );
                if (inactivePauseResult.IsFailed)
                    return inactivePauseResult.LogError();
                continue;
            }

            var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey, cancellationToken);
            if (stopResult.IsFailed)
                return stopResult.LogError();

            var statusUpdateResult = await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                downloadTaskKey,
                DownloadStatus.Paused,
                cancellationToken
            );
            if (statusUpdateResult.IsFailed)
                return statusUpdateResult.LogError();
        }

        return Result.Ok();
    }
}
