using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.Domain
{
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, IProgress<long> progress, int bufferSize = 0x1000, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            long totalRead = 0;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                //Thread.Sleep(10);
                progress.Report(totalRead);
            }
        }
    }
}
