namespace PlexRipper.DownloadManager.Common
{
    public class DownloadWorkerComplete
    {
        public int Id { get; }


        public string FileName { get; set; }

        public int DownloadSpeedAverage { get; set; }

        public long BytesReceived { get; set; }

        public long BytesReceivedGoal { get; set; }

        public bool ReceivedAllBytes => BytesReceived == BytesReceivedGoal;

        public DownloadWorkerComplete(int id)
        {
            Id = id;
        }
    }
}