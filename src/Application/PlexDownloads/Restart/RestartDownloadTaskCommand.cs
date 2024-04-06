using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Restart the <see cref="DownloadTaskGeneric"/> by deleting the PlexDownloadClient and starting a new one.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to restart.</param>
/// <returns>Is successful.</returns>
public record RestartDownloadTaskCommand(Guid DownloadTaskGuid) : IRequest<Result>;

public class RestartDownloadTaskCommandValidator : AbstractValidator<RestartDownloadTaskCommand>
{
    public RestartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class RestartDownloadTaskCommandHandler : IRequestHandler<RestartDownloadTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly INotificationsService _notificationsService;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;
    private readonly IDirectorySystem _directorySystem;

    public RestartDownloadTaskCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        INotificationsService notificationsService,
        IDownloadTaskScheduler downloadTaskScheduler,
        IDirectorySystem directorySystem)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _notificationsService = notificationsService;
        _downloadTaskScheduler = downloadTaskScheduler;
        _directorySystem = directorySystem;
    }

    public async Task<Result> Handle(RestartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTaskGuid = command.DownloadTaskGuid;

        var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskGuid, cancellationToken: cancellationToken);
        var downloadTaskKey = downloadTask.ToKey();

        // Ensure the downloadTask is not currently downloading
        if (downloadTask.IsDownloadable && downloadTask.DownloadStatus == DownloadStatus.Downloading)
        {
            var stopDownloadTasksResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey, cancellationToken);
            if (stopDownloadTasksResult.IsFailed)
                return stopDownloadTasksResult.ToResult().LogError();

            _log.Debug("Deleting partially downloaded files of {DownloadTaskFullTitle}", downloadTask.FullTitle);

            var removeTempResult = _directorySystem.DeleteAllFilesFromDirectory(downloadTask.DownloadDirectory);
            if (removeTempResult.IsFailed)
                return removeTempResult;
        }

        // TODO delete downloadWorkertasks

        // TODO delete filetasks

        // Reset progress of the downloadTask
        await _dbContext.UpdateDownloadProgress(downloadTaskKey, new DownloadTaskProgress
        {
            Percentage = 0,
            DataReceived = 0,
            DataTotal = downloadTask.DataTotal,
            DownloadSpeed = 0,
        }, cancellationToken);

        await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Queued);

        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTaskKey), cancellationToken);

        await _mediator.Publish(new CheckDownloadQueueNotification(downloadTask.PlexServerId), cancellationToken);

        return Result.Ok();
    }
}