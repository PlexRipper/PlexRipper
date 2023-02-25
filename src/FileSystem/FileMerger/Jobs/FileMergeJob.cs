using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Application.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using Environment;
using Logging.Interface;
using PlexRipper.FileSystem.Common;
using Quartz;

namespace PlexRipper.FileSystem;

public class FileMergeJob : IJob
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IFileMergeSystem _fileMergeSystem;
    private readonly INotificationsService _notificationsService;
    private readonly IFileMergeStreamProvider _fileMergeStreamProvider;

    public FileMergeJob(
        ILog log,
        IMediator mediator,
        IFileMergeSystem fileMergeSystem,
        INotificationsService notificationsService,
        IFileMergeStreamProvider fileMergeStreamProvider)
    {
        _log = log;
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
        _log.Here()
            .Debug("Executing job: {NameOfFileMergeJob} for {NameOfFileTaskId} with id: {FileTaskId}", nameof(FileMergeJob), nameof(fileTaskId), fileTaskId);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
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

            _log.Information("Executing {NameOfFileMergeJob} with name {FileTaskFileName} and id {FileTaskId}", nameof(FileMergeJob), fileTask.FileName,
                fileTaskId);

            if (!fileTask.FilePaths.Any())
            {
                _log.Error("File task: {FileName} with id {FileTaskId} did not have any file paths to merge", fileTask.FileName, fileTask.Id);
                return;
            }

            var newDownloadStatus = DownloadStatus.Merging;
            if (fileTask.FilePaths.Count == 1)
                newDownloadStatus = DownloadStatus.Moving;

            downloadTask.DownloadStatus = newDownloadStatus;
            downloadTask.DownloadWorkerTasks.ForEach(x => x.DownloadStatus = newDownloadStatus);

            await _mediator.Send(new UpdateDownloadTasksByIdCommand(downloadTask), token);
            await _mediator.Publish(new DownloadTaskUpdated(downloadTask), token);

            // Verify all file paths exists
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
            try
            {
                // Create FileMergeProgress from bytes received progress
                _bytesReceivedProgress
                    .TakeUntil(x => x == fileTask.FileSize)
                    .Sample(TimeSpan.FromSeconds(1), _timeContext)
                    .Select(dataTransferred =>
                    {
                        var elapsedTime = DateTime.UtcNow.Subtract(transferStarted);
                        return new FileMergeProgress
                        {
                            Id = fileTask.Id,
                            DataTransferred = dataTransferred,
                            DataTotal = fileTask.FileSize,
                            DownloadTaskId = fileTask.DownloadTaskId,
                            PlexLibraryId = fileTask.DownloadTask.PlexLibraryId,
                            PlexServerId = fileTask.DownloadTask.PlexServerId,
                            TransferSpeed = DataFormat.GetTransferSpeed(dataTransferred, elapsedTime.TotalSeconds),
                        };
                    })
                    .SelectMany(async data => await _mediator.Publish(new FileMergeProgressNotification(data), token).ToObservable())
                    .Subscribe();

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

                _log.Debug("Combining {FilePathsCount} into a single file", fileTask.FilePaths.Count);

                await _fileMergeStreamProvider.MergeFiles(fileTask.FilePaths, outputStream, _bytesReceivedProgress, token);

                await outputStream.DisposeAsync();
            }
            catch (Exception e)
            {
                await _notificationsService.SendResult(Result.Fail(new ExceptionalError(e)).LogError());
            }

            // Ensure the progress subscription has finished
            await Task.Delay(1000, token);

            // Clean-up
            _bytesReceivedProgress.OnCompleted();
            _timeContext.Dispose();
            _bytesReceivedProgress.Dispose();
            _log.Here().Information("Finished combining {FilePathsCount} files into {FileTaskFileName}", fileTask.FilePaths.Count, fileTask.FileName);

            await _mediator.Publish(new FileMergeFinishedNotification(fileTaskId), token);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }
}