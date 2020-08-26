using System.Collections.Generic;
using System.Linq;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Enums;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadStatusChanged
    {
        public int Id { get; }
        public DownloadStatus Status { get; }

        public DownloadStatusChanged(int id, DownloadStatus downloadStatus)
        {
            Id = id;
            Status = downloadStatus;
        }
    }
}