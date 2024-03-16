using Application.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Pause a currently downloading <see cref="DownloadTaskGeneric"/>.
/// </summary>
/// <param name="DownloadTaskId">The id of the <see cref="DownloadTaskGeneric"/> to pause.</param>
/// <returns>Is successful.</returns>
public record PauseDownloadTaskCommand(Guid DownloadTaskId) : IRequest<Result>;

public class PauseDownloadTaskCommandValidator : AbstractValidator<PauseDownloadTaskCommand>
{
    public PauseDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskId).NotEmpty();
    }
}

public class PauseDownloadTaskCommandHandler : IRequestHandler<PauseDownloadTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public PauseDownloadTaskCommandHandler(ILog log, IPlexRipperDbContext dbContext, IMediator mediator, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(PauseDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTaskId = command.DownloadTaskId;
        _log.Information("Pausing DownloadTask with id {DownloadTaskFullTitle} from downloading", downloadTaskId);

        var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskId);
        if (stopResult.IsFailed)
            return stopResult.LogError();

        await _downloadTaskScheduler.AwaitDownloadTaskJob(downloadTaskId, cancellationToken);

        _log.Debug("DownloadTask {DownloadTaskId} has been Paused, meaning no downloaded files have been deleted", downloadTaskId);

        // Update the download task status
        await _dbContext.SetDownloadStatus(downloadTaskId, DownloadStatus.Paused);

        var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskId, cancellationToken: cancellationToken);
        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTask.ToKey()), cancellationToken);

        return Result.Ok();
    }
}