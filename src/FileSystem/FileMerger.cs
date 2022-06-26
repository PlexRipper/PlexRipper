using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using Environment;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.FileManager.Command;
using PlexRipper.Application.FileManager.Queries;
using PlexRipper.Domain;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.FileSystem
{
    public class FileMerger : IFileMerger
    {
        private readonly IMediator _mediator;

        private readonly IFileMergeSystem _fileMergeSystem;

        private readonly INotificationsService _notificationsService;

        private readonly IFileMergeStreamProvider _fileMergeStreamProvider;

        #region Fields

        private readonly Channel<DownloadFileTask> _channel = Channel.CreateUnbounded<DownloadFileTask>();

        private readonly Subject<FileMergeProgress> _fileMergeProgressSubject = new();

        private readonly Subject<DownloadFileTask> _fileMergeStartSubject = new();

        private readonly Subject<FileMergeProgress> _fileMergeCompletedSubject = new();

        private readonly CancellationToken _token = new();

        private bool _isExecutingFileTask;

        private Task<Task> _copyTask;

        #endregion

        #region Constructors

        public FileMerger(IMediator mediator, IFileMergeSystem fileMergeSystem, INotificationsService notificationsService,
            IFileMergeStreamProvider fileMergeStreamProvider)
        {
            _mediator = mediator;
            _fileMergeSystem = fileMergeSystem;
            _notificationsService = notificationsService;
            _fileMergeStreamProvider = fileMergeStreamProvider;
        }

        #endregion

        public IObservable<DownloadFileTask> FileMergeStartObservable => _fileMergeStartSubject.AsObservable();

        public IObservable<FileMergeProgress> FileMergeProgressObservable => _fileMergeProgressSubject.AsObservable();

        public IObservable<FileMergeProgress> FileMergeCompletedObservable => _fileMergeCompletedSubject.AsObservable();

        public bool IsBusy => _channel.Reader.Count > 0 && _isExecutingFileTask;

        #region Methods

        #region Private

        public async Task ExecuteFileTasks()
        {
            Log.Information("Running FileTask executor");

            while (!_token.IsCancellationRequested)
            {
                var fileTask = await _channel.Reader.ReadAsync(_token);
                _isExecutingFileTask = true;
                if (!fileTask.FilePaths.Any())
                {
                    Log.Error($"File task: {fileTask.FileName} with id {fileTask.Id} did not have any file paths to merge");
                    return;
                }

                Log.Information($"Executing FileTask {fileTask.FileName} with id {fileTask.Id}");

                _fileMergeStartSubject.OnNext(fileTask);

                foreach (var path in fileTask.FilePaths)
                {
                    if (!_fileMergeSystem.FileExists(path))
                    {
                        var result = Result.Fail($"Filepath: {path} does not exist and cannot be used to merge/move the file!").LogError();
                        await _notificationsService.SendResult(result);
                        return;
                    }
                }

                var transferStarted = DateTime.UtcNow;
                var _timeContext = new EventLoopScheduler();
                Subject<long> _bytesReceivedProgress = new Subject<long>();
                var lastProgress = new FileMergeProgress();

                // Create FileMergeProgress from bytes received progress
                _bytesReceivedProgress
                    .TakeUntil(x => x == fileTask.FileSize)
                    .Sample(TimeSpan.FromSeconds(1), _timeContext)
                    .Select(dataTransferred =>
                    {
                        TimeSpan ElapsedTime = DateTime.UtcNow.Subtract(transferStarted);
                        lastProgress = new FileMergeProgress
                        {
                            Id = fileTask.Id,
                            DataTransferred = dataTransferred,
                            DataTotal = fileTask.FileSize,
                            DownloadTaskId = fileTask.DownloadTaskId,
                            PlexLibraryId = fileTask.DownloadTask.PlexLibraryId,
                            PlexServerId = fileTask.DownloadTask.PlexServerId,
                            TransferSpeed = DataFormat.GetTransferSpeed(dataTransferred, ElapsedTime.TotalSeconds),
                        };
                        return lastProgress;
                    })
                    .Subscribe(data => _fileMergeProgressSubject.OnNext(data), () => { _timeContext.Dispose(); });

                try
                {
                    var streamResult = await _fileMergeStreamProvider.CreateMergeStream(fileTask.DownloadTask.DestinationDirectory);
                    if (streamResult.IsFailed)
                    {
                        streamResult.LogError();
                        continue;
                    }

                    // Merge files
                    var outputStream = streamResult.Value;
                    if (EnvironmentExtensions.IsIntegrationTestMode())
                    {
                        outputStream = new ThrottledStream(streamResult.Value, 5000);
                    }

                    Log.Debug($"Combining {fileTask.FilePaths.Count} into a single file");

                    // TODO Make merge able to be canceled with token
                    await _fileMergeStreamProvider.MergeFiles(fileTask.FilePaths, outputStream, _bytesReceivedProgress);

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
                await _mediator.Send(new DeleteFileTaskByIdCommand(fileTask.Id));
                Log.Information($"Finished combining {fileTask.FilePaths.Count} files into {fileTask.FileName}");
                _fileMergeCompletedSubject.OnNext(lastProgress);
                _isExecutingFileTask = false;
            }
        }

        #endregion

        #region Public

        public async Task<Result> SetupAsync()
        {
            _copyTask = Task.Factory.StartNew(ExecuteFileTasks, TaskCreationOptions.LongRunning);

            if (_copyTask.IsFaulted)
            {
                return Result.Fail("ExecuteFileTasks failed due to an error").LogError();
            }

            await ResumeFileTasks();

            return Result.Ok();
        }

        private async Task ResumeFileTasks()
        {
            var fileTasksResult = await _mediator.Send(new GetAllFileTasksQuery());

            foreach (var downloadFileTask in fileTasksResult.Value)
            {
                await _channel.Writer.WriteAsync(downloadFileTask);
            }
        }

        /// <summary>
        /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
        /// </summary>
        /// <param name="downloadTaskId"></param>
        public async Task<Result> AddFileTaskFromDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId == 0)
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId)).LogError();

            var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true));
            if (downloadTask.IsFailed)
                return downloadTask.ToResult().LogError();

            Log.Debug($"Adding DownloadTask {downloadTask.Value.Title} to a FileTask to be merged");
            var result = await _mediator.Send(new AddFileTaskFromDownloadTaskCommand(downloadTask.Value));
            if (result.IsFailed)
            {
                return result.ToResult().LogError();
            }

            var fileTask = await _mediator.Send(new GetFileTaskByIdQuery(result.Value));
            if (fileTask.IsFailed)
                return fileTask.ToResult().LogError();

            await _channel.Writer.WriteAsync(fileTask.Value);
            return Result.Ok();
        }

        #endregion

        #endregion
    }
}