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
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.FileManager.Command;
using PlexRipper.Application.FileManager.Queries;
using PlexRipper.Domain;

namespace PlexRipper.FileSystem
{
    public class FileManager : IFileManager
    {
        private readonly IMediator _mediator;

        #region Fields

        private readonly Channel<FileTask> _channel = Channel.CreateUnbounded<FileTask>();
        private readonly Subject<FileMergeProgress> _fileMergeProgressSubject = new Subject<FileMergeProgress>();
        private readonly CancellationToken _token = new CancellationToken();
        private Task<Task> _copytask;

        #endregion

        #region Constructors

        public FileManager(IMediator mediator)
        {
            _mediator = mediator;
            _copytask = Task.Factory.StartNew(ExecuteFileTasks, TaskCreationOptions.LongRunning);
        }

        #endregion

        public IObservable<FileMergeProgress> FileMergeProgressObservable => _fileMergeProgressSubject.AsObservable();

        #region Methods

        #region Private

        private async Task ExecuteFileTasks()
        {
            Log.Debug("Running FileTask executor");
            await foreach (var fileTask in _channel.Reader.ReadAllAsync(_token))
            {
                if (!fileTask.FilePaths.Any())
                {
                    return;
                }

                Log.Debug($"Executing FileTask {fileTask.Id}");
                foreach (var path in fileTask.FilePaths)
                {
                    if (!File.Exists(path))
                    {
                        Log.Error($"Filepath: {path} does not exist!");
                        return;
                    }
                }

                var transferStarted = DateTime.UtcNow;
                var _timeContext = new EventLoopScheduler();
                Subject<long> _bytesReceivedProgress = new Subject<long>();

                // Create FileMergeProgress from bytes received progress
                _bytesReceivedProgress
                    .TakeUntil(x => x == fileTask.FileSize)
                    .Sample(TimeSpan.FromSeconds(1), _timeContext)
                    .Select(dataTransferred =>
                    {
                        TimeSpan ElapsedTime = DateTime.UtcNow.Subtract(transferStarted);

                        return new FileMergeProgress
                        {
                            Id = fileTask.Id,
                            DataTransferred = dataTransferred,
                            DataTotal = fileTask.FileSize,
                            DownloadTaskId = fileTask.DownloadTaskId,
                            TransferSpeed = DataFormat.GetDownloadSpeed(dataTransferred, ElapsedTime.TotalSeconds)
                        };
                    })
                    .Subscribe(data => _fileMergeProgressSubject.OnNext(data), () => { _timeContext.Dispose(); });

                try
                {
                    // Merge files
                    await using (var outputStream = File.Create(fileTask.OutputFilePath, 4096, FileOptions.SequentialScan))
                    {
                        Log.Debug($"Combining {fileTask.FilePaths.Count} into a single file");
                        await StreamExtensions.CopyMultipleToAsync(fileTask.FilePaths, outputStream, _bytesReceivedProgress);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    throw;
                }

                // Clean-up
                _bytesReceivedProgress.OnCompleted();
                _bytesReceivedProgress.Dispose();
                Log.Information($"Finished combining {fileTask.FilePaths.Count} files into {fileTask.FileName}");
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to be added as a <see cref="FileTask"/>.</param>
        public async Task<Result> AddFileTask(DownloadTask downloadTask)
        {
            Log.Debug($"Adding DownloadTask {downloadTask.Title} to a FileTask to be merged");
            var result = await _mediator.Send(new AddFileTaskFromDownloadTaskCommand(downloadTask));
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

            _channel.Writer.TryWrite(fileTask.Value);
            return Result.Ok(true);
        }

        #endregion

        #endregion
    }
}