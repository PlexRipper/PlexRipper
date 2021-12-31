﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.FileSystem
{
    public class FileMergeStreamProvider : IFileMergeStreamProvider
    {
        private readonly IFileSystem _fileSystem;

        private readonly INotificationsService _notificationsService;

        public FileMergeStreamProvider(IFileSystem fileSystem, INotificationsService notificationsService)
        {
            _fileSystem = fileSystem;
            _notificationsService = notificationsService;
        }

        public async Task<Result<Stream>> CreateMergeStream(string destinationDirectory)
        {
            // Ensure destination directory exists and is otherwise created.
            var result = _fileSystem.CreateDirectoryFromFilePath(destinationDirectory);
            if (result.IsFailed)
            {
                await _notificationsService.SendResult(result);
                result.LogError();
            }

            return _fileSystem.Create(destinationDirectory, 4096, FileOptions.SequentialScan);
        }

        public async Task MergeFiles(List<string> filePaths, Stream destination, Subject<long> bytesReceivedProgress,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            long totalRead = 0;
            foreach (var filePath in filePaths)
            {
                await using (var inputStream = File.OpenRead(filePath))
                {
                    try
                    {
                        var buffer = new byte[0x1000];
                        int bytesRead;
                        while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            cancellationToken.ThrowIfCancellationRequested();
                            totalRead += bytesRead;
                            bytesReceivedProgress.OnNext(totalRead);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Exception during downloading of ", ex);
                        throw;
                    }
                }

                Log.Debug($"The file at {filePath} has been combined into");
                Delete(filePath);
            }

            bytesReceivedProgress.OnNext(totalRead);
        }

        public Result Delete(string filePath)
        {
            Log.Debug($"Deleting file {filePath} since it has been merged already");
            return _fileSystem.DeleteFile(filePath);
        }
    }
}