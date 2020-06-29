using Microsoft.Extensions.Hosting;
using PlexRipper.Application.Common.Interfaces.Application;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager
{
    public class DownloadWorker : BackgroundService
    {
        private readonly IDownloadManager _downloadManager;
        private readonly ITestClass _testClass;

        public DownloadWorker(IDownloadManager downloadManager, ITestClass testClass)
        {
            _downloadManager = downloadManager;
            _testClass = testClass;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //Log.Debug($"Worker running at: {DateTimeOffset.Now}");
                // _testClass.TestLogging();
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
