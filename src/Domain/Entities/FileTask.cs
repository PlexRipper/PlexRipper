using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace PlexRipper.Domain
{
    public class FileTask: BaseEntity
    {
        #region Constructors

        public FileTask()
        {

        }

        public FileTask(DownloadTask downloadTask)
        {
            DownloadTaskId = downloadTask.Id;
            DownloadTask = downloadTask;
            DestinationFolder = downloadTask.DestinationFolder;
            DestinationFolderId = downloadTask.DestinationFolderId;
            TempDirectory = downloadTask.TempDirectory;
        }

        #endregion

        #region Properties

        public string TempDirectory { get; set; }

        public DateTime CreatedAt { get; set; }

        #region Relationships

        public DownloadTask DownloadTask { get; set; }
        public int DownloadTaskId { get; set; }

        public FolderPath DestinationFolder { get; set; }
        public int DestinationFolderId { get; set; }
        #endregion

        #region Helpers

        [NotMapped]
        public string FileName => DownloadTask?.FileName;

        [NotMapped]
        public string OutputFilePath => Path.Combine(TempDirectory, FileName);

        [NotMapped]
        public long FileSize => DownloadTask?.DataTotal ?? -1L;

        [NotMapped]
        public List<string> FilePaths => DownloadTask?.DownloadWorkerTasks?.Select(x => x.TempFilePath)?.ToList() ?? new List<string>();

        #endregion

        #endregion
    }
}