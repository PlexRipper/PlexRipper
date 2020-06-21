using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager
{
    public class DownloadWorker : BackgroundService
    {
        private readonly IDownloadManager _downloadManager;
        private readonly ILogger<DownloadWorker> _logger;

        public DownloadWorker(IDownloadManager downloadManager, ILogger<DownloadWorker> logger)
        {
            _downloadManager = downloadManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
