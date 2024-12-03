using Data.Contracts;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace PlexRipper.Application;

public class FileMergeJob : IJob
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IDirectorySystem _directorySystem;

    public FileMergeJob(ILog log, IMediator mediator, IPlexRipperDbContext dbContext, IDirectorySystem directorySystem)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _directorySystem = directorySystem;
    }

    public static string DownloadTaskIdParameter => "DownloadTaskId";

    public static JobKey GetJobKey(Guid id) => new($"{DownloadTaskIdParameter}_{id}", nameof(FileMergeJob));

    public async Task Execute(IJobExecutionContext context)
    {
        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var dataMap = context.JobDetail.JobDataMap;
            var downloadTaskKey = dataMap.GetJsonValue<DownloadTaskKey>(DownloadTaskIdParameter);
            if (downloadTaskKey is null)
            {
                ResultExtensions.IsNull(nameof(DownloadTaskKey)).LogError();
                return;
            }

            _log.Here()
                .Debug(
                    "Executing job: {NameOfFileMergeJob} for {NameOfFileTaskId} with id: {FileTaskId}",
                    nameof(FileMergeJob),
                    nameof(downloadTaskKey),
                    downloadTaskKey.Id
                );

            var result = await _mediator.Send(
                new MergeFilesFromFileTaskCommand(downloadTaskKey),
                context.CancellationToken
            );

            if (result.IsFailed)
            {
                return;
            }

            var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, context.CancellationToken);

            if (downloadTask!.DownloadStatus is DownloadStatus.MoveFinished or DownloadStatus.MergeFinished)
            {
                CleanUpTempFolders(downloadTask);

                await _dbContext.SetDownloadStatus(downloadTaskKey, DownloadStatus.Completed);

                // Clean up the DownloadWorkerTasks
                await _dbContext
                    .DownloadWorkerTasks.Where(x => x.DownloadTaskId == downloadTask.Id)
                    .ExecuteDeleteAsync();

                await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTaskKey));
            }
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }

    private void CleanUpTempFolders(DownloadTaskFileBase downloadTask)
    {
        if (downloadTask.DownloadTaskType == DownloadTaskType.EpisodeData)
        {
            // This deletes the Season folder
            _directorySystem.DeleteDirectoryFromFilePath(downloadTask.FilePaths.First());

            // This deletes the TvShow folder
            _directorySystem.DeleteDirectoryFromFilePath(downloadTask.DownloadDirectory);
        }

        if (downloadTask.DownloadTaskType == DownloadTaskType.MovieData)
        {
            // This deletes the Season folder
            _directorySystem.DeleteDirectoryFromFilePath(downloadTask.FilePaths.First());
        }
    }
}
