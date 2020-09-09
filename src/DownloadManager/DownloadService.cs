using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager
{
    public class DownloadService : BackgroundService
    {
        public DownloadService()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Log.Debug($"Worker running at: {DateTimeOffset.Now}");
                // _testClass.TestLogging();
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
