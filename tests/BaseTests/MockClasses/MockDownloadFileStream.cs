using System.IO;
using FluentResults;
using Logging;
using PlexRipper.DownloadManager;

namespace PlexRipper.BaseTests
{
    public class MockDownloadFileStream : IDownloadFileStream
    {
        private readonly ITestStreamTracker _testStreamTracker;

        public MockDownloadFileStream(ITestStreamTracker testStreamTracker)
        {
            _testStreamTracker = testStreamTracker;
        }

        public Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize)
        {
            Log.Debug("In memory download stream is created");
            var stream = new MemoryStream();
            _testStreamTracker.AddDownloadFileStream(stream);
            return Result.Ok((Stream)stream);
        }
    }
}