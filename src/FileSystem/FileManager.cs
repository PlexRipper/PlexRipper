using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        #endregion

        #region Constructors

        public FileManager(IMediator mediator)
        {
            _mediator = mediator;
            Task.Factory.StartNew(ExecuteFileTasks, TaskCreationOptions.LongRunning);
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
                var _progress = new Progress<long>();
                _progress.ProgressChanged += (sender, l) =>
                {
                    var progress = new FileMergeProgress
                    {
                        FileTaskId = fileTask.Id,
                        BytesTransferred = l,
                        BytesTotal = fileTask.FileSize
                    };
                    _fileMergeProgressSubject.OnNext(progress);
                };

                using (var outputStream = File.Create(fileTask.OutputFilePath, 2048, FileOptions.Asynchronous))
                {
                    if (!fileTask.FilePaths.Any())
                    {
                        return;
                    }

                    Log.Debug($"Combining {fileTask.FilePaths.Count} into a single file");
                    foreach (var filePath in fileTask.FilePaths)
                    {
                        using (var inputStream = File.OpenRead(filePath))
                        {
                            // Buffer size can be passed as the second argument.
                            await inputStream.CopyToAsync(outputStream, _progress, 2048);
                        }

                        Log.Debug($"The file at {filePath} has been combined into {fileTask.FileName}");
                        File.Delete(filePath);
                    }
                }

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