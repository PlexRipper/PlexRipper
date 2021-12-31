using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.BaseTests
{
    public class MockFileMergeStreamProvider : IFileMergeStreamProvider
    {
        private readonly ITestStreamTracker _testStreamTracker;

        public MockFileMergeStreamProvider(ITestStreamTracker testStreamTracker)
        {
            _testStreamTracker = testStreamTracker;
        }

        public async Task<Result<Stream>> CreateMergeStream(string destinationDirectory)
        {
            Log.Verbose("In memory file merge stream is created");
            var stream = new MemoryStream();
            _testStreamTracker.AddFileMergeStream(stream);
            return Result.Ok((Stream)stream);
        }

        public async Task MergeFiles(List<string> filePaths, Stream destination, Subject<long> bytesReceivedProgress,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var bytesDone = 1000 * 1024;
            foreach (var _ in filePaths)
            {
                for (var i = 0; i < 2; i++)
                {
                    bytesReceivedProgress.OnNext(bytesDone += 1000 * 1024);
                    await Task.Delay(1000);
                }
            }
        }
    }
}