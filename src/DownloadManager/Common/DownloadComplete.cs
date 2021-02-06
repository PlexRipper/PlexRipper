using System.Collections.Generic;
using System.Linq;
using PlexRipper.Domain;

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

        public int PlexServerId => DownloadTask.PlexServerId;

        public int PlexLibraryId => DownloadTask.PlexLibraryId;

        public string FileName => DownloadTask.FileName;

        public List<string> FilePaths => DownloadWorkerCompletes.Select(x => x.FilePath).ToList();

        #endregion

        public DownloadStatusChanged ToStatus()
        {
            return new DownloadStatusChanged
            {
                Id = Id,
                Status = DownloadStatus.Completed,
                PlexLibraryId = PlexLibraryId,
                PlexServerId = PlexServerId,
            };
        }
    }
}