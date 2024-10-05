using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Stops and disposes of the PlexDownloadClient executing the <see cref="DownloadTaskGeneric"/> if it is downloading.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to stop.</param>
/// <returns>If successful a list of the DownloadTasks that were stopped.</returns>
public record StopDownloadTaskCommand(Guid DownloadTaskGuid) : IRequest<Result>;

public class StopDownloadTaskCommandValidator : AbstractValidator<StopDownloadTaskCommand>
{
    public StopDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StopDownloadTaskCommandHandler : IRequestHandler<StopDownloadTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IDirectorySystem _directorySystem;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public StopDownloadTaskCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IDirectorySystem directorySystem,
        IMediator mediator,
        IDownloadTaskScheduler downloadTaskScheduler
    )
    {
        _log = log;
        _dbContext = dbContext;
        _directorySystem = directorySystem;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(StopDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        var downloadTasks = await _dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);

        foreach (var downloadTaskKey in downloadTasks)
        {
            var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
                continue;
            }

            _log.Information("Stopping {DownloadTaskFullTitle} from downloading", downloadTask.FullTitle);

            if (await _downloadTaskScheduler.IsDownloading(downloadTaskKey, cancellationToken))
            {
                var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey, cancellationToken);
                if (stopResult.IsFailed)
                {
                    // Since this command is done per server, we can abort since there will at most be 1 download task downloading at a time and if that fails we can't continue
                    return stopResult.LogError();
                }
            }

            _log.Debug("Deleting partially downloaded files of {DownloadTaskFullTitle}", downloadTask.FullTitle);

            var removeTempResult = _directorySystem.DeleteAllFilesFromDirectory(downloadTask.DownloadDirectory);
            if (removeTempResult.IsFailed)
                return removeTempResult;

            // Update the download task status
            await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Stopped);

            // Delete all worker tasks
            await _dbContext
                .DownloadWorkerTasks.Where(x => x.DownloadTaskId == downloadTaskKey.Id)
                .ExecuteDeleteAsync(cancellationToken);

            // Reset the download progress
            downloadTask.DataReceived = 0;
            downloadTask.Percentage = 0;
            await _dbContext.UpdateDownloadProgress(downloadTaskKey, downloadTask, cancellationToken);

            // TODO delete file tasks but first check if already merging

            await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTaskKey), cancellationToken);
        }

        return Result.Ok();
    }
}
