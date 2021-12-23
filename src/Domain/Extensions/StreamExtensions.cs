using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResultExtensions.lib;
using Logging;

namespace PlexRipper.Domain
{
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, Subject<long> bytesReceivedProgress, int bufferSize = 0x1000,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var buffer = new byte[bufferSize];
                int bytesRead;
                long totalRead = 0;
                while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
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

        public static async Task CopyMultipleToAsync(List<string> filePaths, Stream destination, Subject<long> bytesReceivedProgress,
            int bufferSize = 0x1000, bool deletePartEveryLoop = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            long totalRead = 0;
            for (int i = 0; i < filePaths.Count; i++)
            {
                var filePath = filePaths[i];
                await using (var inputStream = File.OpenRead(filePath))
                {
                    try
                    {
                        var buffer = new byte[bufferSize];
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

                if (deletePartEveryLoop)
                {
                    File.Delete(filePath);
                }
            }

            bytesReceivedProgress.OnNext(totalRead);
        }

        public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                }
                catch (IOException)
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }

                    Thread.Sleep(50);
                }
            }

            return null;
        }
    }
}