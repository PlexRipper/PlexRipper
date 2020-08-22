using PlexRipper.DownloadManager.Common;

namespace PlexRipper.DownloadManager.Download
{
    public class DownloadWorker
    {
        public DownloadRange DownloadRange { get; set; }
        public DownloadWorker()
        {

        }

        public void AssignDownloadRange(DownloadRange downloadRange)
        {
            if (downloadRange != null)
            {
                DownloadRange = downloadRange;
            }
        }

        public void Start()
        {

        }
    }
}