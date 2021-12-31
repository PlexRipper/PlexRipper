using System;
using System.Text;
using System.Threading.Tasks;
using Logging;
using PlexRipper.Application;
using PlexRipper.DownloadManager;
using PlexRipper.FileSystem;

namespace PlexRipper.BaseTests
{
    public class TestApplicationTracker : ITestApplicationTracker
    {
        private readonly IFileMerger _fileMerger;

        private readonly IDownloadTracker _downloadTracker;

        private readonly IDownloadQueue _downloadQueue;

        public TestApplicationTracker(IFileMerger fileMerger, IDownloadTracker downloadTracker, IDownloadQueue downloadQueue)
        {
            _fileMerger = fileMerger;
            _downloadTracker = downloadTracker;
            _downloadQueue = downloadQueue;
        }

        private bool CheckIfBusy(bool log = false)
        {
            if (log)
            {
                var status = new StringBuilder();
                status.Append("Application status - ");
                status.Append($"[{nameof(DownloadQueue)}: {_downloadQueue.IsBusy}] ");
                status.Append($"[{nameof(DownloadTracker)}: {_downloadTracker.IsBusy}] ");
                status.Append($"[{nameof(FileMerger)}: {_fileMerger.IsBusy}] ");
                Log.Debug(status.ToString());
            }

            return !_fileMerger.IsBusy && !_downloadTracker.IsBusy && !_downloadQueue.IsBusy;
        }

        public async Task WaitUntilApplicationIsIdle(int checkInterval = 2000, bool logStatus = false)
        {
            var loopIndex = 0;
            while (true)
            {
                await Task.Delay(checkInterval);
                if (CheckIfBusy(logStatus))
                {
                    Log.Information($"Application is idle again, resuming test after a wait of {checkInterval / 1000 * (loopIndex + 1)} seconds.");
                    return;
                }

                if (loopIndex++ > 60)
                {
                    Log.Fatal("Application was never idle after 120 seconds");
                    throw new TimeoutException();
                }
            }
        }
    }
}