using System.IO;
using FluentResults;
using Logging;
using PlexRipper.BaseTests.MockClasses;
using PlexRipper.DownloadManager;

namespace PlexRipper.BaseTests
{
    public class MockDownloadFileStream : IDownloadFileStream
    {
        private readonly ITestNotifier _testNotifier;

        public MockDownloadFileStream(ITestNotifier testNotifier)
        {
            _testNotifier = testNotifier;
        }

        public Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize)
        {
            Log.Debug("In memory download stream is created");
            var stream = new MemoryStream();
            _testNotifier.AddDownloadFileStream(stream);
            return Result.Ok((Stream)stream);
        }
    }
}