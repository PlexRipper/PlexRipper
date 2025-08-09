using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FileSystem.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

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
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;
    private readonly IFileMergeScheduler _fileMergeScheduler;

    public StartDownloadTaskCommandHandler(
        IPlexRipperDbContext dbContext,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        IDownloadTaskScheduler downloadTaskScheduler,
        IFileMergeScheduler fileMergeScheduler
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _downloadTaskScheduler = downloadTaskScheduler;
        _fileMergeScheduler = fileMergeScheduler;
    }

    public async Task<Result> ExecuteAsync(StartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        // TODO: Improve performance by fetching ALL download tasks in one query
        var downloadableChildTaskKeys = await _dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);
        if (!downloadableChildTaskKeys.Any())
            return ResultExtensions.IsEmpty(nameof(downloadableChildTaskKeys)).LogWarning();

        var nextDownloadTaskKey = downloadableChildTaskKeys.First();
        var nextDownloadTask = await _dbContext.GetDownloadTaskFileAsync(nextDownloadTaskKey, cancellationToken);
        if (nextDownloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskFileBase), nextDownloadTaskKey.Id).LogError();

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

                    // TODO: - This should be done in the DownloadJob
                    await _dbContext.SetDownloadStatus(nextDownloadTaskKey, DownloadStatus.Downloading);

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
                if (!(await _fileMergeScheduler.IsDownloadTaskMerging(nextDownloadTaskKey)))
                {
                    await _fileMergeScheduler.StartFileMergeJob(nextDownloadTaskKey);
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

        await _commandExecutor.Send(new DownloadTaskUpdatedNotification(key), cancellationToken);

        await _eventPublisher.PublishAsync(new CheckDownloadQueueNotification(key.PlexServerId), cancellationToken);

        return Result.Ok();
    }
}
