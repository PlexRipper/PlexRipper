using FastEndpoints;
using FluentValidation;
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

    public PauseDownloadTaskCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IDownloadTaskScheduler downloadTaskScheduler,
        IMoveDownloadFileScheduler moveDownloadFileScheduler
    )
    {
        _log = log.ForContext<PauseDownloadTaskCommandHandler>();
        _dbContext = dbContext;
        _downloadTaskScheduler = downloadTaskScheduler;
        _moveDownloadFileScheduler = moveDownloadFileScheduler;
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

            _log.Here()
                .Information("Pausing DownloadTask with id {DownloadTaskTitle} from downloading", downloadTask.Title);

            if (downloadTask.DownloadTaskPhase == DownloadTaskPhase.Completed)
                continue;

            if (downloadTask.DownloadTaskPhase == DownloadTaskPhase.FileTransfer)
            {
                if (!await _moveDownloadFileScheduler.IsDownloadFileMoving(downloadTaskKey))
                    continue;

                var stopMoveResult = await _moveDownloadFileScheduler.StopMoveDownloadFileJob(downloadTaskKey);
                if (stopMoveResult.IsFailed)
                    return stopMoveResult.LogError();

                var resetMoveProgressResult = await _dbContext.ResetDownloadTaskProgress(
                    downloadTaskKey,
                    DownloadStatus.MovePaused,
                    cancellationToken
                );
                if (resetMoveProgressResult.IsFailed)
                    return resetMoveProgressResult.LogError();
                continue;
            }

            if (!await _downloadTaskScheduler.IsDownloading(downloadTaskKey, cancellationToken))
                continue;

            var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey, cancellationToken);
            if (stopResult.IsFailed)
                return stopResult.LogError();

            var resetDownloadProgressResult = await _dbContext.ResetDownloadTaskProgress(
                downloadTaskKey,
                DownloadStatus.Paused,
                cancellationToken
            );
            if (resetDownloadProgressResult.IsFailed)
                return resetDownloadProgressResult.LogError();
        }

        return Result.Ok();
    }
}
