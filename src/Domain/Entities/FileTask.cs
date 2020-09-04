using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using PlexRipper.Domain.Entities.Base;

namespace PlexRipper.Domain.Entities
{
    public class FileTask: BaseEntity
    {
        #region Constructors

        public FileTask()
        {

        }

        public FileTask(string outputDirectory, string fileName, int downloadTaskId)
        {
            OutputDirectory = outputDirectory;
            FileName = fileName;
            DownloadTaskId = downloadTaskId;
        }

        #endregion

        #region Properties

        public string FileName { get; set; }
        public string OutputDirectory { get; set; }

        public DateTime CreatedAt { get; set; }
        #region Relationships

        public DownloadTask DownloadTask { get; set; }
        public int DownloadTaskId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public string OutputFilePath => Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(FileName), FileName);

        [NotMapped]
        public long FileSize => DownloadTask?.DataTotal ?? -1L;

        [NotMapped]
        public List<string> FilePaths => DownloadTask?.DownloadWorkerTasks?.Select(x => x.TempFilePath)?.ToList() ?? new List<string>();

        #endregion

        #endregion
    }
}