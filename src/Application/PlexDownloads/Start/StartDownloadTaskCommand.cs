using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

public record StartDownloadTaskCommand(Guid DownloadTaskGuid) : IRequest<Result>;

public class StartDownloadTaskCommandValidator : AbstractValidator<StartDownloadTaskCommand>
{
    public StartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StartDownloadTaskCommandHandler : IRequestHandler<StartDownloadTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public StartDownloadTaskCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IDownloadTaskScheduler downloadTaskScheduler
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(StartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        var downloadableChildTaskKeys = await _dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);

        var nextDownloadTaskKey = downloadableChildTaskKeys.FirstOrDefault();

        // Check if this server is already downloading something and then pause it
        var activeDownloadKeys = await _downloadTaskScheduler.GetCurrentlyDownloadingKeysByServer(key.PlexServerId);
        if (activeDownloadKeys.Any())
        {
            // avoid pausing if the download task is marked to be started
            foreach (var downloadKey in activeDownloadKeys)
                if (downloadKey != nextDownloadTaskKey)
                    await _mediator.Send(new PauseDownloadTaskCommand(downloadKey.Id), cancellationToken);
        }

        for (var i = 0; i < downloadableChildTaskKeys.Count; i++)
        {
            var downloadTaskKey = downloadableChildTaskKeys[i];
            var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
                continue;
            }

            if (i > 0)
            {
                if (downloadTask.DownloadStatus == DownloadStatus.Paused)
                    await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Queued);
                continue;
            }

            // Start the download task
            if (!await _downloadTaskScheduler.IsDownloading(downloadTaskKey, cancellationToken))
            {
                _log.Information("Starting DownloadTask with id: {DownloadTaskGuid}", downloadTaskKey.Id);
                var startResult = await _downloadTaskScheduler.StartDownloadTaskJob(downloadTaskKey);
                if (startResult.IsFailed)
                    return startResult.LogError();

                await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Downloading);
            }
        }

        await _mediator.Send(new DownloadTaskUpdatedNotification(key), cancellationToken);
        await _mediator.Publish(new CheckDownloadQueueNotification(key.PlexServerId), cancellationToken);

        return Result.Ok();
    }
}
