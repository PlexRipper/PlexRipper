using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.FileManager.Command;
using PlexRipper.Application.FileManager.Queries;
using PlexRipper.Domain;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types.FileSystem;

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
                    .Subscribe(data => _fileMergeProgressSubject.OnNext(data), () =>
                    {
                        _timeContext.Dispose();
                    });

                // Merge files
                await using (var outputStream = File.Create(fileTask.OutputFilePath, 4096, FileOptions.SequentialScan))
                {
                    Log.Debug($"Combining {fileTask.FilePaths.Count} into a single file");
                    await StreamExtensions.CopyMultipleToAsync(fileTask.FilePaths, outputStream, _bytesReceivedProgress);
                }

                // Clean-up
                _bytesReceivedProgress.OnCompleted();
                _bytesReceivedProgress.Dispose();
                Log.Information($"Finished combining {fileTask.FilePaths.Count} files into {fileTask.FileName}");
            }
        }

        #endregion

        #region Public

        public void AddFileTask(FileTask fileTask)
        {
            _channel.Writer.TryWrite(fileTask);
        }

        /// <summary>
        /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to be added as a <see cref="FileTask"/>.</param>
        public void AddFileTask(DownloadTask downloadTask)
        {
            Task.Run(async () =>
            {
                Log.Debug($"Adding DownloadTask {downloadTask.Title} to a FileTask to be merged");
                var result = await _mediator.Send(new AddFileTaskFromDownloadTaskCommand(downloadTask));
                if (result.IsFailed)
                {
                    // TODO Add notification here for front-end
                    result.LogError();
                    return;
                }

                var fileTask = await _mediator.Send(new GetFileTaskByIdQuery(result.Value));
                if (fileTask.IsFailed)
                {
                    fileTask.LogError();
                    return;
                }

                AddFileTask(fileTask.Value);
            });
        }

        #endregion

        #endregion
    }
}