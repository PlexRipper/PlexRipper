using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Domain.Entities;
using PlexRipper.DownloadManager.Common;
using PlexRipper.DownloadManager.Download;
using Serilog;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager
{
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IUserSettings _userSettings;
        private readonly IDownloadTaskRepository _downloadTaskRepository;

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

        public DownloadManager(IUserSettings userSettings, ILogger logger, IDownloadTaskRepository downloadTaskRepository)
        {
            _userSettings = userSettings;
            _downloadTaskRepository = downloadTaskRepository;
            Log = logger;
        }

        #endregion Constructors

        #region Methods


        private PlexDownloadClient CreateDownloadClient(DownloadTask downloadTask)
        {
            PlexDownloadClient newClient = new PlexDownloadClient(downloadTask, this, _userSettings, Log);

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

        public async Task StartDownloadAsync(DownloadTask downloadTask)
        {
            // Add to DB
            await _downloadTaskRepository.AddAsync(downloadTask);

            Log.Debug(downloadTask.ToString());
            var downloadClient = CreateDownloadClient(downloadTask);
            downloadClient.Start();
        }


        #endregion Methods

    }
}
