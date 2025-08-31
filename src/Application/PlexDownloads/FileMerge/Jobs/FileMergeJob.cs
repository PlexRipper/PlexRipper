using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Data.Contracts;
using Reaparr.Logging;

namespace Reaparr.Application;

public class FileMergeJob : IJob
{
    private readonly ILog _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;

    public FileMergeJob(ILog log, ICommandExecutor commandExecutor, IReaparrDbContext dbContext)
    {
        _log = log;
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(FileMergeJob));

    public async Task Execute(IJobExecutionContext context)
    {
        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        var dataMap = context.JobDetail.JobDataMap;
        var ct = context.CancellationToken;
        var downloadTaskKey = dataMap.GetJsonValue<DownloadTaskKey>(DownloadTaskIdParameter);
        if (downloadTaskKey is null)
        {
            ResultExtensions.IsNull(nameof(DownloadTaskKey)).LogError();
            return;
        }

        try
        {
            _log.Here()
                .Information(
                    "Executing job: {NameOfFileMergeJob} for {NameOfFileTaskId} with id: {FileTaskId}",
                    nameof(FileMergeJob),
                    nameof(downloadTaskKey),
                    downloadTaskKey.Id
                );

            var result = await _commandExecutor.Send(new MergeFilesFromFileTaskCommand(downloadTaskKey), ct);

            if (result.IsFailed)
            {
                _log.Error("Failed to merge all files for {DownloadTaskKey}", downloadTaskKey);
                return;
            }

            var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, ct);

            if (downloadTask!.DownloadStatus is DownloadStatus.MoveFinished or DownloadStatus.MergeFinished)
            {
                await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Completed);

                // Clean up the DownloadWorkerTasks
                await _commandExecutor.Send(new CleanUpDownloadTaskFoldersCommand(downloadTaskKey), ct);

                await _dbContext
                    .DownloadWorkerTasks.Where(x => x.DownloadTaskId == downloadTask.Id)
                    .ExecuteDeleteAsync(ct);

                await _commandExecutor.Send(new DownloadTaskUpdatedCommand(downloadTaskKey), ct);
            }
        }
        catch (TaskCanceledException)
        {
            _log.Warning("{JobName} for {DownloadTaskKey} was cancelled", nameof(FileMergeJob), downloadTaskKey);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}
