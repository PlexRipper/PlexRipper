using System;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Application.FileManager.Command;
using PlexRipper.Application.FileManager.Queries;
using PlexRipper.Domain;

namespace PlexRipper.FileSystem
{
    public class FileMerger : IFileMerger
    {
        private readonly IMediator _mediator;

        private readonly IFileSystem _fileSystem;

        private readonly INotificationsService _notificationsService;

        #region Fields

        private readonly Channel<DownloadFileTask> _channel = Channel.CreateUnbounded<DownloadFileTask>();

        private readonly Subject<FileMergeProgress> _fileMergeProgressSubject = new();

        private readonly Subject<FileMergeProgress> _fileMergeCompletedSubject = new();

        private readonly CancellationToken _token = new CancellationToken();

        private Task<Task> _copyTask;

        #endregion

        #region Constructors

        public FileMerger(IMediator mediator, IFileSystem fileSystem, INotificationsService notificationsService)
        {
            _mediator = mediator;
            _fileSystem = fileSystem;
            _notificationsService = notificationsService;
        }

        #endregion

        public IObservable<FileMergeProgress> FileMergeProgressObservable => _fileMergeProgressSubject.AsObservable();

        public IObservable<FileMergeProgress> FileMergeCompletedObservable => _fileMergeCompletedSubject.AsObservable();

        #region Methods

        #region Private

        public async Task ExecuteFileTasks()
        {
            Log.Information("Running FileTask executor");

            while (!_token.IsCancellationRequested)
            {
                var fileTask = await _channel.Reader.ReadAsync(_token);

                if (!fileTask.FilePaths.Any())
                {
                    Log.Error($"File task: {fileTask.FileName} with id {fileTask.Id} did not have any file paths to merge");
                    return;
                }

                Log.Debug($"Executing FileTask {fileTask.FileName} with id {fileTask.Id}");
                foreach (var path in fileTask.FilePaths)
                {
                    if (!File.Exists(path))
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
                    // Ensure destination directory exists and is otherwise created.
                    var result = _fileSystem.CreateDirectoryFromFilePath(fileTask.DownloadTask.DestinationDirectory);
                    if (result.IsFailed)
                    {
                        await _notificationsService.SendResult(result);
                        result.LogError();
                        continue;
                    }

                    // Merge files
                    await using (var outputStream = File.Create(fileTask.DownloadTask.DestinationDirectory, 4096, FileOptions.SequentialScan))
                    {
                        Log.Debug($"Combining {fileTask.FilePaths.Count} into a single file");
                        await StreamExtensions.CopyMultipleToAsync(fileTask.FilePaths, outputStream, _bytesReceivedProgress);
                        _fileSystem.DeleteDirectoryFromFilePath(fileTask.FilePaths.First());
                    }
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
            }
        }

        #endregion

        #region Public

        public async Task<Result> SetupAsync()
        {
            _copyTask = Task.Factory.StartNew(ExecuteFileTasks, TaskCreationOptions.LongRunning);

            if (_copyTask.IsFaulted)
            {
                return Result.Fail("ExecuteFileTasks failed due to an error");
            }

            await ResumeFileTasks();

            return Result.Ok(true);
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
            {
                return ResultExtensions.IsInvalidId(nameof(downloadTaskId)).LogError();
            }

            var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
            if (downloadTask.IsFailed)
            {
                return downloadTask.LogError();
            }

            Log.Debug($"Adding DownloadTask {downloadTask.Value.Title} to a FileTask to be merged");
            var result = await _mediator.Send(new AddFileTaskFromDownloadTaskCommand(downloadTask.Value));
            if (result.IsFailed)
            {
                // TODO Add notification here for front-end
                return result.LogError();
            }

            var fileTask = await _mediator.Send(new GetFileTaskByIdQuery(result.Value));
            if (fileTask.IsFailed)
            {
                return fileTask.LogError();
            }

            await _channel.Writer.WriteAsync(fileTask.Value);
            return Result.Ok(true);
        }

        #endregion

        #endregion
    }
}