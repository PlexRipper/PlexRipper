using Microsoft.Extensions.Hosting;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager
{
    public class DownloadWorker : BackgroundService
    {
        private readonly IDownloadManager _downloadManager;

        public DownloadWorker(IDownloadManager downloadManager)
        {
            _downloadManager = downloadManager;
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
