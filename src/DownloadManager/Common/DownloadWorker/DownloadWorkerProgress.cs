using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Domain.Common;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadWorkerProgress : IDownloadWorkerProgress
    {
        public int Id { get; }

        // public string Status { get; set; }

        public long DataReceived { get; }

        public long DataTotal { get; }

        public int DownloadSpeed { get; }

        public int DownloadSpeedAverage { get; }

        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        /// <summary>
        /// The time remaining in seconds for this DownloadWorker to finish.
        /// </summary>
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        public long BytesRemaining => DataTotal - DataReceived;

        public bool IsCompleted => DataReceived == DataTotal;

        public decimal Percentage => DataFormat.GetPercentage(DataReceived, DataTotal);
        public DownloadWorkerProgress(int id, long dataReceived, long dataTotal, int downloadSpeed, int downloadSpeedAverage)
        {
            Id = id;
            DataReceived = dataReceived;
            DataTotal = dataTotal;
            DownloadSpeed = downloadSpeed;
            DownloadSpeedAverage = downloadSpeedAverage;
        }
    }
}