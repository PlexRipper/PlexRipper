using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Application.Common.Models;
using PlexRipper.DownloadManager.Common;
using PlexRipper.DownloadManager.Download;
using Serilog;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace PlexRipper.DownloadManager
{
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IUserSettings _userSettings;

        private ILogger Log { get; }

        // Collection which contains all download clients, bound to the DataGrid control
        public List<PlexDownloadClient> DownloadsList = new List<PlexDownloadClient>();

        #endregion Fields

        #region Properties

        // Number of currently active downloads
        public int ActiveDownloads
        {
            get
            {
                int active = 0;
                //foreach (WebDownloadClient d in DownloadsList)
                //{
                //    if (!d.HasError)
                //        if (d.Status == DownloadStatus.Waiting || d.Status == DownloadStatus.Downloading)
                //            active++;
                //}
                return active;
            }
        }

        // Number of completed downloads
        public int CompletedDownloads
        {
            get
            {
                int completed = 0;
                //foreach (WebDownloadClient d in DownloadsList)
                //{
                //    if (d.Status == DownloadStatus.Completed)
                //        completed++;
                //}
                return completed;
            }
        }

        // Total number of downloads in the list
        public int TotalDownloads => DownloadsList.Count;

        #endregion Properties

        #region Constructors

        public DownloadManager(IUserSettings userSettings, ILogger logger)
        {
            _userSettings = userSettings;
            Log = logger.ForContext<DownloadManager>();
        }

        #endregion Constructors

        #region Methods


        private PlexDownloadClient CreateDownloadClient()
        {
            PlexDownloadClient newClient = new PlexDownloadClient(this, _userSettings, Log);

            newClient.DownloadProgressChanged += OnDownloadProgressChanged;
            newClient.DownloadFileCompleted += OnDownloadFileCompleted;

            DownloadsList.Add(newClient);
            return newClient;
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Log.Information("The download has completed!");
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

            Log.Information($"Downloaded {DataFormat.FormatSizeString(e.BytesReceived)} of {DataFormat.FormatSizeString(e.TotalBytesToReceive)} bytes. {e.ProgressPercentage} % complete...");
        }

        public void StartDownload(DownloadRequest downloadRequest)
        {
            var downloadClient = CreateDownloadClient();
            downloadClient.Start(downloadRequest);
        }


        #endregion Methods

    }
}
