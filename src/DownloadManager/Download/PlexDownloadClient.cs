using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Application.Common.Models;
using PlexRipper.DownloadManager.Common;
using PlexRipper.PlexApi.Api;
using Serilog;
using System;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager.Download
{
    public class PlexDownloadClient : ExtendedWebClient
    {
        public ILogger Log { get; }
        private IDownloadManager _downloadManager;
        private IUserSettings _userSettings;

        public decimal Percentage { get; internal set; }

        // Size of downloaded data which was written to the local file
        public long DownloadedSize { get; set; }

        // Percentage of downloaded data

        public PlexDownloadClient(IDownloadManager downloadManager, IUserSettings userSettings, ILogger logger)
        {
            Log = logger;
            _downloadManager = downloadManager;
            _userSettings = userSettings;

            AddHeaders();
        }

        private void AddHeaders()
        {
            foreach ((string key, string value) in PlexHeaderData.GetBasicHeaders)
            {
                this.Headers.Add(key, value);
            }
        }
        public Task DownloadTask { get; set; }

        // Start or continue download
        public void Start(DownloadRequest downloadRequest)
        {
            Log.Debug(downloadRequest.DownloadUrl);
            try
            {
                Task.WaitAll(DownloadFileTaskAsync(downloadRequest.DownloadUri, downloadRequest.FileName));
            }
            catch (Exception e)
            {
                Log.Error(e, $"Could not download {downloadRequest.FileName} from {downloadRequest.DownloadUrl}");
                throw;
            }
        }

        public void Cancel()
        {
            CancelAsync();
        }

        //protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        //{

        //    if (e.ProgressPercentage)
        //    {
        //        base.OnDownloadProgressChanged(e);
        //    }
        //}
    }
}
