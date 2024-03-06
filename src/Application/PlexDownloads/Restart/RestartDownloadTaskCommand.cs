using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Restart the <see cref="DownloadTask"/> by deleting the PlexDownloadClient and starting a new one.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTask"/> to restart.</param>
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
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public RestartDownloadTaskCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(RestartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTaskGuid = command.DownloadTaskGuid;
        var stopDownloadTasksResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskGuid);
        if (stopDownloadTasksResult.IsFailed)
            return stopDownloadTasksResult.ToResult().LogError();

        // TODO Implement RegenerateDownloadTask

        // var regeneratedDownloadTasks = await RegenerateDownloadTask(new List<Guid>() { command.DownloadTaskGuid });
        // if (regeneratedDownloadTasks.IsFailed)
        //     return regeneratedDownloadTasks.LogError();
        //
        // var downloadTasks = regeneratedDownloadTasks.Value;
        // var restartedDownloadTask = downloadTasks.Flatten(x => x.Children).FirstOrDefault(x => x.Id == downloadTaskGuid);
        // if (restartedDownloadTask == null)
        //     return ResultExtensions.IsInvalidId(nameof(downloadTaskGuid), downloadTaskGuid).LogError();
        //
        // await _dbContext.UpdateDownloadTasksAsync(downloadTasks, cancellationToken);
        //
        // await _dbContext.SetDownloadStatus(downloadTaskGuid, DownloadStatus.Queued, cancellationToken);
        //
        // // Notify of a DownloadTask being updated
        // var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskGuid, cancellationToken: cancellationToken);
        // if (downloadTask is null)
        //     return ResultExtensions.EntityNotFound(nameof(DownloadTask), command.DownloadTaskGuid).LogError();
        //
        // await _mediator.Send(new DownloadTaskUpdated(downloadTask.ToKey()), cancellationToken);
        //
        // await _mediator.Publish(new CheckDownloadQueue(downloadTask.PlexServerId), cancellationToken);

        return Result.Ok();
    }

    /// <summary>
    /// Regenerates <see cref="DownloadTask">DownloadTasks</see> while maintaining the Guid and priority.
    /// Will also remove old <see cref="DownloadWorkerTask">DownloadWorkerTasks</see> assigned to the old downloadTasks from the database.
    /// </summary>
    /// <param name="downloadTaskGuids"></param>
    /// <returns>A list of newly generated <see cref="DownloadTask">DownloadTasks</see></returns>
    private async Task<Result<List<DownloadTaskGeneric>>> RegenerateDownloadTask(List<Guid> downloadTaskGuids)
    {
        if (!downloadTaskGuids.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTaskGuids)).LogWarning();

        _log.Debug("Regenerating {DownloadTaskIdsCount} download tasks", downloadTaskGuids.Count);

        var freshDownloadTasks = new List<DownloadTaskGeneric>();

        await Task.CompletedTask;

        throw new NotImplementedException();

        // foreach (var downloadTaskId in downloadTaskGuids)
        // {
        //     var downloadTask = await _dbContext.DownloadTasks.GetAsync(downloadTaskId);
        //     if (downloadTask is null)
        //     {
        //         ResultExtensions.EntityNotFound(nameof(DownloadTask), downloadTaskId).LogError();
        //         continue;
        //     }
        //
        //     var mediaIdResult = await _dbContext.GetPlexMediaByMediaKeyAsync(downloadTask.Id, downloadTask.PlexServerId, downloadTask.MediaType);
        //     if (mediaIdResult.IsFailed)
        //     {
        //         var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}");
        //         result.WithReasons(mediaIdResult.Reasons);
        //         await _notificationsService.SendResult(result);
        //         continue;
        //     }
        //
        //     var list = new List<DownloadMediaDTO>
        //     {
        //         new()
        //         {
        //             Type = downloadTask.MediaType,
        //             MediaIds = new List<int> { mediaIdResult.Value },
        //         },
        //     };
        //
        //     var downloadTasksResult = await GenerateAsync(list);
        //     if (downloadTasksResult.IsFailed)
        //     {
        //         var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}").WithReasons(mediaIdResult.Reasons);
        //         await _notificationsService.SendResult(result);
        //         continue;
        //     }
        //
        //        await dbContext.DownloadWorkerTasks
        //     .Where(x => x.DownloadTaskId == downloadTaskId)
        //     .ExecuteDeleteAsync(cancellationToken);

        //     downloadTasksResult.Value[0].Id = downloadTask.Id;
        //     downloadTasksResult.Value[0].Priority = downloadTask.Priority;
        //
        //     freshDownloadTasks.AddRange(downloadTasksResult.Value);
        // }

        _log.Debug("Successfully regenerated {FreshDownloadTasksCount} out of {DownloadTaskIdsCount} download tasks", freshDownloadTasks.Count,
            downloadTaskGuids.Count);

        if (downloadTaskGuids.Count - freshDownloadTasks.Count > 0)
            _log.ErrorLine("Failed to generate");

        return Result.Ok(freshDownloadTasks);
    }
}