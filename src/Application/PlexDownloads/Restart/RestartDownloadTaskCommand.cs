using BackgroundServices.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

/// <summary>
/// Restart the <see cref="DownloadTask"/> by deleting the PlexDownloadClient and starting a new one.
/// </summary>
/// <param name="DownloadTaskId">The id of the <see cref="DownloadTask"/> to restart.</param>
/// <returns>Is successful.</returns>
public record RestartDownloadTaskCommand(int DownloadTaskId) : IRequest<Result>;

public class RestartDownloadTaskCommandValidator : AbstractValidator<RestartDownloadTaskCommand>
{
    public RestartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskId).GreaterThan(0);
    }
}

public class RestartDownloadTaskCommandHandler : IRequestHandler<RestartDownloadTaskCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskFactory _downloadTaskFactory;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public RestartDownloadTaskCommandHandler(
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IDownloadTaskFactory downloadTaskFactory,
        IDownloadTaskScheduler downloadTaskScheduler)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskFactory = downloadTaskFactory;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(RestartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTaskId = command.DownloadTaskId;
        var stopDownloadTasksResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskId);
        if (stopDownloadTasksResult.IsFailed)
            return stopDownloadTasksResult.ToResult().LogError();

        var regeneratedDownloadTasks = await _downloadTaskFactory.RegenerateDownloadTask(new List<int>() { command.DownloadTaskId });
        if (regeneratedDownloadTasks.IsFailed)
            return regeneratedDownloadTasks.LogError();

        var downloadTasks = regeneratedDownloadTasks.Value;
        var restartedDownloadTask = downloadTasks.Flatten(x => x.Children).FirstOrDefault(x => x.Id == downloadTaskId);
        if (restartedDownloadTask == null)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogError();

        await _mediator.Send(new UpdateDownloadTasksByIdCommand(downloadTasks), cancellationToken);

        await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(downloadTaskId, DownloadStatus.Queued), cancellationToken);

        // Notify of a DownloadTask being updated
        var downloadTask = await _dbContext.DownloadTasks.GetAsync(downloadTaskId, cancellationToken);
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), command.DownloadTaskId).LogError();

        await _mediator.Send(new DownloadTaskUpdated(downloadTask), cancellationToken);

        await _mediator.Publish(new CheckDownloadQueue(downloadTask.PlexServerId), cancellationToken);

        return Result.Ok();
    }
}