using PlexRipper.Domain.Common;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadWorkerProgress
    {
        public int Id { get; }

        // public string Status { get; set; }

        public long DataReceived { get; }

        public long DataTotal { get; }

        public bool IsCompleted { get; set; }

        public decimal Percentage => DataFormat.GetPercentage(DataReceived, DataTotal);
        public DownloadWorkerProgress(int id, long dataReceived, long dataTotal)
        {
            Id = id;
            DataReceived = dataReceived;
            DataTotal = dataTotal;
        }
    }
}