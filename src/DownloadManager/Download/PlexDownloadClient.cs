using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Application.Common.Models;
using PlexRipper.DownloadManager.Common;
using PlexRipper.PlexApi.Api;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager.Download
{
    public class PlexDownloadClient : ExtendedWebClient
    {

        #region Fields

        private IDownloadManager _downloadManager;
        private IUserSettings _userSettings;
        #endregion Fields

        #region Constructors

        public PlexDownloadClient(IDownloadManager downloadManager, IUserSettings userSettings, ILogger logger)
        {
            Log = logger;
            _downloadManager = downloadManager;
            _userSettings = userSettings;

            AddHeaders();
        }

        #endregion Constructors

        #region Properties

        // Size of downloaded data which was written to the local file
        public long DownloadedSize { get; set; }

        public Task DownloadTask { get; set; }
        public ILogger Log { get; }
        public decimal Percentage { get; internal set; }
        public long BytesReceived { get; internal set; }
        public long TotalBytesToReceive { get; internal set; }



        #endregion Properties

        #region Methods

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

        private void AddHeaders()
        {
            foreach ((string key, string value) in PlexHeaderData.GetBasicHeaders)
            {
                this.Headers.Add(key, value);
            }
        }

        #endregion Methods

        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            BytesReceived = e.BytesReceived;
            TotalBytesToReceive = e.TotalBytesToReceive;
            var newPercentage = DataFormat.GetPercentage(BytesReceived, TotalBytesToReceive);
            if (newPercentage != Percentage)
            {

                base.OnDownloadProgressChanged(e);
            }
        }
    }
}
