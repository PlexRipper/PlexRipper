using System.Reactive.Subjects;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.BaseTests;

public class MockFileMergeStreamProvider : IFileMergeStreamProvider
{
    private readonly ITestStreamTracker _testStreamTracker;

    public MockFileMergeStreamProvider(ITestStreamTracker testStreamTracker)
    {
        _testStreamTracker = testStreamTracker;
    }

#pragma warning disable CS1998
    public async Task<Result<Stream>> OpenOrCreateMergeStream(string fileDestinationPath)
    {
        var stream = new MemoryStream();
        _testStreamTracker.AddFileMergeStream(stream);
        return Result.Ok((Stream)stream);
    }
#pragma warning restore CS1998

    public async Task MergeFiles(
        List<string> filePaths,
        Stream destination,
        Subject<long> bytesReceivedProgress,
        CancellationToken cancellationToken = default)
    {
        var bytesDone = 1000 * 1024;
        foreach (var _ in filePaths)
            for (var i = 0; i < 10; i++)
            {
                bytesReceivedProgress.OnNext(bytesDone += 1000 * 1024);
                await Task.Delay(500, cancellationToken);
            }
    }
}