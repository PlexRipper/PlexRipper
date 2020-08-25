using System.Collections.Generic;
using PlexRipper.Domain.Common;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadWorkerComplete
    {
        public int Id { get; }
        public string FilePath { get; set; }
        public string DestinationPath { get; set; }
        public string FileName { get; set; }
        public int DownloadSpeedAverage { get; set; }

        public DownloadWorkerComplete(int id)
        {
            Id = id;
        }
    }
}