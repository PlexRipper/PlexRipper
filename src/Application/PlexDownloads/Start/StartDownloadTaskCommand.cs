using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application;

public record StartDownloadTaskCommand(Guid DownloadTaskGuid) : ICommand<Result>;

public class StartDownloadTaskCommandValidator : AbstractValidator<StartDownloadTaskCommand>
{
    public StartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StartDownloadTaskCommandHandler : ICommandHandler<StartDownloadTaskCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;
    private readonly IMoveDownloadFileScheduler _moveDownloadFileScheduler;

    public StartDownloadTaskCommandHandler(
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        IEventPublisher eventPublisher,
        IDownloadTaskScheduler downloadTaskScheduler,
        IMoveDownloadFileScheduler moveDownloadFileScheduler
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
        _eventPublisher = eventPublisher;
        _downloadTaskScheduler = downloadTaskScheduler;
        _moveDownloadFileScheduler = moveDownloadFileScheduler;
    }

    public async Task<Result> ExecuteAsync(StartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        var downloadableChildTasks = await _dbContext.GetDownloadableChildTasks(key, cancellationToken);
        if (!downloadableChildTasks.Any())
            return ResultExtensions.IsEmpty(nameof(downloadableChildTasks)).LogWarning();

        var nextDownloadTask = downloadableChildTasks.FirstOrDefault(x =>
            x.DownloadStatus == DownloadStatus.Paused || x.DownloadStatus == DownloadStatus.MovePaused
        );
        nextDownloadTask ??= downloadableChildTasks.FirstOrDefault(x =>
            x.DownloadTaskPhase != DownloadTaskPhase.Completed
        );
        nextDownloadTask ??= downloadableChildTasks.First();
        var nextDownloadTaskKey = nextDownloadTask.ToKey();

        if (await _dbContext.IsDownloadsPausedByUser(nextDownloadTaskKey.PlexServerId))
        {
            return Result.Fail("Download tasks cannot be started while the server is paused by the user").LogWarning();
        }

        if (key.Type is DownloadTaskType.TvShow or DownloadTaskType.Season)
        {
            var statusesToQueue = nextDownloadTask.DownloadStatus switch
            {
                DownloadStatus.Paused => new[] { DownloadStatus.Paused, DownloadStatus.MovePaused },
                DownloadStatus.MovePaused => new[] { DownloadStatus.Paused, DownloadStatus.MovePaused },
                DownloadStatus.Stopped => new[] { DownloadStatus.Stopped },
                _ => [],
            };

            foreach (
                var waitingTask in downloadableChildTasks.Where(x =>
                    x.Id != nextDownloadTask.Id && statusesToQueue.Contains(x.DownloadStatus)
                )
            )
                await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                    waitingTask.ToKey(),
                    DownloadStatus.Queued,
                    cancellationToken
                );
        }

        // Start the download task depending on the phase
        switch (nextDownloadTask.DownloadTaskPhase)
        {
            case DownloadTaskPhase.None:
            case DownloadTaskPhase.Downloading:
                if (!await _downloadTaskScheduler.IsDownloading(nextDownloadTaskKey, cancellationToken))
                {
                    var startResult = await _downloadTaskScheduler.StartDownloadTaskJob(nextDownloadTaskKey);
                    if (startResult.IsFailed)
                        return startResult.LogError();

                    var activeDownloadKeys = await _downloadTaskScheduler.GetCurrentlyDownloadingKeysByServer(
                        key.PlexServerId
                    );

                    // Avoid pausing the download task that just started
                    foreach (var downloadKey in activeDownloadKeys.Where(x => x != nextDownloadTaskKey))
                        await _commandExecutor.Send(new PauseDownloadTaskCommand(downloadKey.Id), cancellationToken);
                }

                break;

            case DownloadTaskPhase.FileTransfer:
                // Multiple merging tasks can be processing at the same time
                if (!(await _moveDownloadFileScheduler.IsDownloadFileMoving(nextDownloadTaskKey)))
                {
                    await _moveDownloadFileScheduler.StartMoveDownloadFileJob(nextDownloadTaskKey);
                }

                break;

            case DownloadTaskPhase.Completed:
                return Result.Fail("Download task is already completed and cannot be started again").LogWarning();

            case DownloadTaskPhase.Unknown:
                return Result.Fail("Download task is in an unknown phase and cannot be started").LogError();
            default:
                throw new ArgumentOutOfRangeException(
                    $"{nextDownloadTask.DownloadTaskPhase} is not a valid DownloadTaskPhase enum value"
                );
        }

        await _eventPublisher.PublishAsync(new CheckDownloadQueueEvent(key.PlexServerId), cancellationToken);

        return Result.Ok();
    }
}
