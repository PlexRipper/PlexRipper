using FileSystem.Contracts;

namespace PlexRipper.BaseTests;

public class MockDownloadFileStream : IDownloadFileStream
{
    private readonly ITestStreamTracker _testStreamTracker;

    public MockDownloadFileStream(ITestStreamTracker testStreamTracker)
    {
        _testStreamTracker = testStreamTracker;
    }

    public Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize)
    {
        var stream = new MemoryStream();
        _testStreamTracker.AddDownloadFileStream(stream);
        return Result.Ok((Stream)stream);
    }
}
