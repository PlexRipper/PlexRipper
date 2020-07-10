using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.DownloadManager.Common;
using PlexRipper.PlexApi.Api;
using System;
using System.Net;
using System.Threading;
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

        public PlexDownloadClient(DownloadTask downloadTask, IDownloadManager downloadManager, IUserSettings userSettings)
        {
            _downloadManager = downloadManager;
            _userSettings = userSettings;
            DownloadTask = downloadTask;
            AddHeaders();
        }

        #endregion Constructors

        #region Properties

        // Size of downloaded data which was written to the local file
        public long DownloadedSize { get; set; }

        public DownloadTask DownloadTask { get; internal set; }
        /// <summary>
        /// The ClientId is always the same id that is assigned to the <see cref="DownloadTask"/>
        /// </summary>
        public int ClientId => DownloadTask.Id;

        public decimal Percentage { get; internal set; }
        public long BytesReceived { get; internal set; }
        public long TotalBytesToReceive { get; internal set; }

        public CancellationToken CancellationToken { get; set; } = new CancellationToken();

        #endregion Properties

        #region Methods

        /// <summary>
        /// Start the download of the DownloadTask passed during the construction.
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            Log.Debug(DownloadTask.DownloadUrl);
            try
            {
                Task.WaitAll(Task.Run(() => DownloadFileTaskAsync(DownloadTask.DownloadUri, DownloadTask.FileName),
                    CancellationToken));
            }
            catch (Exception e)
            {
                Log.Error(e, $"Could not download {DownloadTask.FileName} from {DownloadTask.DownloadUrl}");
                throw;
            }
        }

        public bool Cancel()
        {
            CancelAsync();
            return CancellationToken.IsCancellationRequested;
        }

        private void AddHeaders()
        {
            foreach ((string key, string value) in PlexHeaderData.GetBasicHeaders)
            {
                this.Headers.Add(key, value);
            }
        }

        protected override void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            BytesReceived = e.BytesReceived;
            TotalBytesToReceive = e.TotalBytesToReceive;
            var newPercentage = DataFormat.GetPercentage(BytesReceived, TotalBytesToReceive);
            // Only fire event when percentage has changed
            if (newPercentage != Percentage)
            {
                base.OnDownloadProgressChanged(e);
            }
            Percentage = newPercentage;
        }

        #endregion Methods
    }
}
