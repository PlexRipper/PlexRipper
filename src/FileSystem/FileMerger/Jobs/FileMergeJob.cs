using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Data.Contracts;
using DownloadManager.Contracts;
using Environment;
using PlexRipper.Application;
using PlexRipper.FileSystem.Common;
using Quartz;

namespace PlexRipper.FileSystem;

public class FileMergeJob : IJob
{
    private readonly IMediator _mediator;
    private readonly IFileMergeSystem _fileMergeSystem;
    private readonly INotificationsService _notificationsService;
    private readonly IFileMergeStreamProvider _fileMergeStreamProvider;

    public FileMergeJob(
        IMediator mediator,
        IFileMergeSystem fileMergeSystem,
        INotificationsService notificationsService,
        IFileMergeStreamProvider fileMergeStreamProvider)
    {
        _mediator = mediator;
        _fileMergeSystem = fileMergeSystem;
        _notificationsService = notificationsService;
        _fileMergeStreamProvider = fileMergeStreamProvider;
    }

    public static string FileTaskId => "FileTaskId";

    public static JobKey GetJobKey(int id)
    {
        return new JobKey($"{FileTaskId}_{id}", nameof(FileMergeJob));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var fileTaskId = dataMap.GetIntValue(FileTaskId);
        var token = context.CancellationToken;

        try
        {
            var fileTaskResult = await _mediator.Send(new GetFileTaskByIdQuery(fileTaskId), token);
            if (fileTaskResult.IsFailed)
            {
                fileTaskResult.LogError();
                return;
            }

            var fileTask = fileTaskResult.Value;
            var downloadTask = fileTask.DownloadTask;

            Log.Information($"Executing {nameof(FileMergeJob)} with name {fileTask.FileName} and id {fileTaskId}");

            if (!fileTask.FilePaths.Any())
            {
                Log.Error($"File task: {fileTask.FileName} with id {fileTask.Id} did not have any file paths to merge");
                return;
            }

            var newDownloadStatus = DownloadStatus.Merging;
            if (fileTask.FilePaths.Count == 1)
                newDownloadStatus = DownloadStatus.Moving;

            downloadTask.DownloadStatus = newDownloadStatus;
            downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = newDownloadStatus);

            await _mediator.Publish(new DownloadStatusChanged(downloadTask.Id, downloadTask.RootDownloadTaskId, newDownloadStatus), token);

            foreach (var path in fileTask.FilePaths)
                if (!_fileMergeSystem.FileExists(path))
                {
                    var result = Result.Fail($"Filepath: {path} does not exist and cannot be used to merge/move the file!").LogError();
                    await _notificationsService.SendResult(result);
                    return;
                }

            var transferStarted = DateTime.UtcNow;
            var _timeContext = new EventLoopScheduler();
            var _bytesReceivedProgress = new Subject<long>();

            // Create FileMergeProgress from bytes received progress
            _bytesReceivedProgress
                .TakeUntil(x => x == fileTask.FileSize)
                .Sample(TimeSpan.FromSeconds(1), _timeContext)
                .Select(dataTransferred =>
                {
                    var ElapsedTime = DateTime.UtcNow.Subtract(transferStarted);
                    return new FileMergeProgress
                    {
                        Id = fileTask.Id,
                        DataTransferred = dataTransferred,
                        DataTotal = fileTask.FileSize,
                        DownloadTaskId = fileTask.DownloadTaskId,
                        PlexLibraryId = fileTask.DownloadTask.PlexLibraryId,
                        PlexServerId = fileTask.DownloadTask.PlexServerId,
                        TransferSpeed = DataFormat.GetTransferSpeed(dataTransferred, ElapsedTime.TotalSeconds),
                    };
                })
                .Subscribe(data => _mediator.Publish(new FileMergeProgressNotification(data), token), () => { _timeContext.Dispose(); });

            try
            {
                var streamResult = await _fileMergeStreamProvider.OpenOrCreateMergeStream(fileTask.DestinationFilePath);
                if (streamResult.IsFailed)
                {
                    streamResult.LogError();
                    return;
                }

                // Merge files
                var outputStream = streamResult.Value;

                if (EnvironmentExtensions.IsIntegrationTestMode())
                    outputStream = new ThrottledStream(streamResult.Value, 5000);

                Log.Debug($"Combining {fileTask.FilePaths.Count} into a single file");

                // TODO Make merge able to be canceled with token
                await _fileMergeStreamProvider.MergeFiles(fileTask.FilePaths, outputStream, _bytesReceivedProgress, token);

                _fileMergeSystem.DeleteDirectoryFromFilePath(fileTask.FilePaths.First());
                await outputStream.DisposeAsync();
            }
            catch (Exception e)
            {
                await _notificationsService.SendResult(Result.Fail(new ExceptionalError(e)).LogError());
            }

            // Clean-up
            _bytesReceivedProgress.OnCompleted();
            _bytesReceivedProgress.Dispose();
            await _mediator.Send(new DeleteFileTaskByIdCommand(fileTask.Id), token);
            await _mediator.Publish(new DownloadStatusChanged(downloadTask.Id, downloadTask.RootDownloadTaskId, DownloadStatus.Completed), token);
            Log.Information($"Finished combining {fileTask.FilePaths.Count} files into {fileTask.FileName}");
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }
}