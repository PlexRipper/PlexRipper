using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Used for the merging and transferring of the completed downloaded media file.
    /// </summary>
    public class DownloadFileTask : BaseEntity
    {
        #region Properties

        public DateTime CreatedAt { get; set; }

        #region Relationships

        public DownloadTask DownloadTask { get; set; }

        public int DownloadTaskId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public string FileName => DownloadTask?.FileName;

        [NotMapped]
        public string DestinationFilePath => DownloadTask.DestinationPath;

        [NotMapped]
        public long FileSize => DownloadTask?.DataTotal ?? -1L;

        [NotMapped]
        public List<string> FilePaths => DownloadTask?.DownloadWorkerTasks?.Select(x => x.TempFilePath)?.ToList() ?? new List<string>();

        #endregion

        #endregion
    }
}