using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Application.Common.Models;
using PlexRipper.DownloadManager.Common;
using Serilog;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager.Download
{
    public class PlexDownloadClient : ExtendedWebClient
    {
        private IDownloadManager _downloadManager;
        private IUserSettings _userSettings;
        private ILogger _logger;

        public PlexDownloadClient(IDownloadManager downloadManager, IUserSettings userSettings, ILogger logger)
        {
            _downloadManager = downloadManager;
            _userSettings = userSettings;
            _logger = logger.ForContext<PlexDownloadClient>();

        }

        public Task DownloadTask { get; set; }

        // Start or continue download
        public void Start(DownloadRequest downloadRequest)
        {
            Log.Debug(downloadRequest.DownloadUrl);
            DownloadTask = DownloadFileTaskAsync(downloadRequest.DownloadUri, downloadRequest.FileName);
            DownloadTask.Start();
        }

        public void Cancel()
        {
            CancelAsync();
        }
    }
}
