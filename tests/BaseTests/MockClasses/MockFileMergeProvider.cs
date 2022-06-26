using System.Reactive.Subjects;
using FluentResults;
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

#pragma warning disable CS1998
        public async Task<Result<Stream>> CreateMergeStream(string destinationDirectory)
        {
            var stream = new MemoryStream();
            _testStreamTracker.AddFileMergeStream(stream);
            return Result.Ok((Stream)stream);
        }
#pragma warning restore CS1998

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