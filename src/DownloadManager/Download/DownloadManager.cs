using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.Settings;
using Serilog;
using System.Collections.Generic;

namespace PlexRipper.DownloadManager.Download
{
    public class DownloadManager : IDownloadManager
    {
        #region Fields

        private readonly IUserSettings _userSettings;

        private readonly ILogger _logger;
        private ILogger Log { get; }

        // Collection which contains all download clients, bound to the DataGrid control
        public List<WebDownloadClient> DownloadsList = new List<WebDownloadClient>();

        #endregion Fields

        #region Properties

        // Number of currently active downloads
        public int ActiveDownloads
        {
            get
            {
                int active = 0;
                foreach (WebDownloadClient d in DownloadsList)
                {
                    if (!d.HasError)
                        if (d.Status == DownloadStatus.Waiting || d.Status == DownloadStatus.Downloading)
                            active++;
                }
                return active;
            }
        }

        // Number of completed downloads
        public int CompletedDownloads
        {
            get
            {
                int completed = 0;
                foreach (WebDownloadClient d in DownloadsList)
                {
                    if (d.Status == DownloadStatus.Completed)
                        completed++;
                }
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


        public WebDownloadClient CreateDownloadClient()
        {
            var newClient = new WebDownloadClient(this, _userSettings, Log);
            DownloadsList.Add(newClient);
            return newClient;
        }

        #endregion Methods

    }
}
