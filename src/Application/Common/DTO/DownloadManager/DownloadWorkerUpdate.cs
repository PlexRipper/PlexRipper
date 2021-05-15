using PlexRipper.Domain;

namespace PlexRipper.Application.Common.DTO.DownloadManager
{
    public class DownloadWorkerUpdate : IDownloadWorkerUpdate
    {
        public int Id { get; set; }

        public DownloadStatus DownloadStatus { get; set; }

        public long DataReceived { get; set; }

        public long DataTotal { get; set; }

        public int DownloadSpeed { get; set; }

        public int DownloadSpeedAverage { get; set; }

        public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

        /// <summary>
        /// The time remaining in seconds for this DownloadWorker to finish.
        /// </summary>
        public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, DownloadSpeed);

        /// <summary>
        /// The time elapsed of this DownloadWorker.
        /// </summary>
        public long TimeElapsed { get; set; }

        public long BytesRemaining => DataTotal - DataReceived;

        public bool IsCompleted => DataReceived == DataTotal;

        public decimal Percentage => DataFormat.GetPercentage(DataReceived, DataTotal);
    }
}