using System.IO.Abstractions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Stops and disposes of the PlexDownloadClient executing the <see cref="DownloadTaskGeneric"/> if it is downloading.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to stop.</param>
/// <returns>If successful a list of the DownloadTasks that were stopped.</returns>
public record StopDownloadTaskCommand(Guid DownloadTaskGuid) : ICommand<Result>;

public class StopDownloadTaskCommandValidator : AbstractValidator<StopDownloadTaskCommand>
{
    public StopDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class StopDownloadTaskCommandHandler : ICommandHandler<StopDownloadTaskCommand, Result>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IFile _file;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public StopDownloadTaskCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IFile file,
        IDownloadTaskScheduler downloadTaskScheduler
    )
    {
        _log = log.ForContext<StopDownloadTaskCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _file = file;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> ExecuteAsync(StopDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var key = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (key is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogError();

        var downloadTasks = await _dbContext.GetDownloadableChildTaskKeys(key, cancellationToken);

        foreach (var downloadTaskKey in downloadTasks)
        {
            var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id).LogError();
                continue;
            }

            _log.Here().Information("Stopping {DownloadTaskFullTitle} from downloading", downloadTask.FullTitle);

            if (await _downloadTaskScheduler.IsDownloading(downloadTaskKey, cancellationToken))
            {
                var stopResult = await _downloadTaskScheduler.StopDownloadTaskJob(downloadTaskKey, cancellationToken);
                if (stopResult.IsFailed)
                {
                    // Since this command is done per server, we can abort since there will at most be 1 download task downloading at a time and if that fails we can't continue
                    return stopResult.LogError();
                }
            }

            _log.Here().Debug("Deleting partially downloaded files of {DownloadTaskFullTitle}", downloadTask.FullTitle);

            foreach (var filePath in downloadTask.FilePaths.Distinct())
            {
                Result.Try(() => _file.Delete(filePath)).LogIfFailed();
            }

            // Delete all worker tasks
            await _dbContext
                .DownloadWorkerTasks.Where(x => x.DownloadTaskId == downloadTaskKey.Id)
                .ExecuteDeleteAsync(cancellationToken);

            // Reset the download progress
            await _dbContext.ResetDownloadTaskProgress(downloadTaskKey, DownloadStatus.Stopped, cancellationToken);

            // TODO: delete file tasks but first check if already merging

            await _commandExecutor.Send(new DownloadTaskUpdatedCommand(downloadTaskKey), cancellationToken);
        }

        return Result.Ok();
    }
}
