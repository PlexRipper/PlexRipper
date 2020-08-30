using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;

namespace PlexRipper.FileSystem
{
    public class FileManager : IFileManagement
    {
        private readonly Progress<long> _progress = new Progress<long>();
        private readonly ConcurrentQueue<FileTask> _queue = new ConcurrentQueue<FileTask>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly Channel<FileTask> _channel = Channel.CreateUnbounded<FileTask>();
        private readonly CancellationToken _token = new CancellationToken();

        public FileManager()
        {
            Task.Factory.StartNew(ExecuteFileTasks, TaskCreationOptions.LongRunning);
        }

        private async Task ExecuteFileTasks()
        {
            Log.Debug("Running FileTask executor");
            await foreach (var fileTask in _channel.Reader.ReadAllAsync(_token))
            {
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

        public void AddFileTask(FileTask fileTask)
        {
            _channel.Writer.TryWrite(fileTask);
        }
    }
}