using System.Diagnostics;
using System.Text;
using PlexRipper.Application;
using PlexRipper.DownloadManager;
using PlexRipper.FileSystem;
using Quartz;

namespace PlexRipper.BaseTests
{
    public class TestApplicationTracker : ITestApplicationTracker
    {
        private readonly IScheduler _scheduler;

        private readonly IFileMerger _fileMerger;

        private readonly IDownloadTracker _downloadTracker;

        private readonly IDownloadQueue _downloadQueue;

        public TestApplicationTracker(IScheduler scheduler, IFileMerger fileMerger, IDownloadTracker downloadTracker, IDownloadQueue downloadQueue)
        {
            _scheduler = scheduler;
            _fileMerger = fileMerger;
            _downloadTracker = downloadTracker;
            _downloadQueue = downloadQueue;
        }

        private async Task<bool> CheckIfBusy(bool log = false)
        {
            var isSchedulerBusy = await IsSchedulerBusy();
            var threadCount = ThreadCount();
            if (log)
            {
                var status = new StringBuilder();
                status.Append("Application status - ");
                status.Append($"[Thread Count: {threadCount}] ");
                status.Append($"[{nameof(DownloadQueue)}: {_downloadQueue.IsBusy}] ");
                status.Append($"[{nameof(DownloadTracker)}: {_downloadTracker.IsBusy}] ");
                status.Append($"[{nameof(FileMerger)}: {_fileMerger.IsBusy}] ");
                status.Append($"[{nameof(IScheduler)}: {isSchedulerBusy}] ");
                Log.Debug(status.ToString());
            }

            return threadCount < 50 && !_fileMerger.IsBusy && !_downloadTracker.IsBusy && !_downloadQueue.IsBusy && !isSchedulerBusy;
        }

        private async Task<bool> IsSchedulerBusy()
        {
            var jobs = await _scheduler.GetCurrentlyExecutingJobs();
            return jobs.Count > 0;
        }

        private int ThreadCount()
        {
            return Process.GetCurrentProcess().Threads.Count;
        }

        public async Task WaitUntilApplicationIsIdle(int checkInterval = 2000, bool logStatus = false)
        {
            var loopIndex = 0;
            while (true)
            {
                var beginCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

                await Task.Delay(checkInterval);

                // if (await CheckIfBusy(logStatus))
                // {
                //     Log.Information($"Application is idle again, resuming test after a wait of {checkInterval / 1000 * (loopIndex + 1)} seconds.");
                //     // return;
                // }

                var endCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
                Log.Debug($"CPU Time: {endCpuTime - beginCpuTime}");
                if (endCpuTime - beginCpuTime < TimeSpan.FromMilliseconds(100))
                {
                    Log.Debug($"Returned CPU Time: {endCpuTime - beginCpuTime}");
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