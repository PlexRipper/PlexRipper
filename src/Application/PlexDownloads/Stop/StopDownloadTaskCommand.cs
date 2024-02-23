using BackgroundServices.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using FileSystem.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Stops and disposes of the PlexDownloadClient executing the <see cref="DownloadTask"/> if it is downloading.
/// </summary>
/// <param name="DownloadTaskId">The id of the <see cref="DownloadTask"/> to stop.</param>
/// <returns>If successful a list of the DownloadTasks that were stopped.</returns>
public record StopDownloadTaskCommand(int DownloadTaskId) : IRequest<Result>;

public class StopDownloadTaskCommandValidator : AbstractValidator<StopDownloadTaskCommand>
{
    public StopDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskId).GreaterThan(0);
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
        IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _directorySystem = directorySystem;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(StopDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.DownloadTasks.GetAsync(command.DownloadTaskId, cancellationToken);
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), command.DownloadTaskId).LogError();

        _log.Information("Stopping {DownloadTaskFullTitle} from downloading", downloadTask.FullTitle);

        var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(command.DownloadTaskId);
        if (stopResult.IsFailed)
            return stopResult;

        downloadTask.DownloadStatus = DownloadStatus.Stopped;

        _log.Debug("Deleting partially downloaded files of {DownloadTaskFullTitle}", downloadTask.FullTitle);

        var removeTempResult = _directorySystem.DeleteAllFilesFromDirectory(downloadTask.DownloadDirectory);
        if (removeTempResult.IsFailed)
            return removeTempResult;

        await _mediator.Send(new DownloadTaskUpdated(downloadTask), cancellationToken);

        return Result.Ok();
    }
}