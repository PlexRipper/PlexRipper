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
        var downloadTask = await _dbContext.GetDownloadTaskAsync(command.DownloadTaskGuid, cancellationToken: cancellationToken);
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        _log.Information("Stopping {DownloadTaskFullTitle} from downloading", downloadTask.FullTitle);

        var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(command.DownloadTaskGuid);
        if (stopResult.IsFailed)
            return stopResult;

        _log.Debug("Deleting partially downloaded files of {DownloadTaskFullTitle}", downloadTask.FullTitle);

        var removeTempResult = _directorySystem.DeleteAllFilesFromDirectory(downloadTask.DownloadDirectory);
        if (removeTempResult.IsFailed)
            return removeTempResult;

        // Update the download task status
        await _dbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.Stopped);

        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTask.ToKey()), cancellationToken);

        return Result.Ok();
    }
}