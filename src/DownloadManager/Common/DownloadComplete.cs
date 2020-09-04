using System.Collections.Generic;
using System.Linq;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Entities;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadComplete
    {
        #region Constructors

        public DownloadComplete(DownloadTask downloadTask)
        {
            DownloadTask = downloadTask;
        }

        #endregion

        #region Properties

        /// <summary>
        /// This id is the same as assigned to the <see cref="DownloadTask"/>.
        /// </summary>
        public int Id => DownloadTask.Id;

        public long DataReceived { get; set; }

        public long DataTotal { get; set; }

        public string DestinationPath { get; set; }

        public DownloadTask DownloadTask { get; internal set; }

        public List<DownloadWorkerComplete> DownloadWorkerCompletes { get; set; }

        public string FileName => DownloadTask.FileName;

        public List<string> FilePaths => DownloadWorkerCompletes.Select(x => x.FilePath).ToList();

        #endregion
    }
}