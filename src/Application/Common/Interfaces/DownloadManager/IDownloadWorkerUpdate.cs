using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IDownloadWorkerUpdate
    {
        int Id { get; set; }

        DownloadStatus DownloadStatus { get; set; }

        long DataReceived { get; set; }

        long DataTotal { get; set; }

        int DownloadSpeed { get; set; }

        int DownloadSpeedAverage { get; set; }

        string DownloadSpeedFormatted { get; }

        /// <summary>
        /// The time remaining in seconds for this DownloadWorker to finish.
        /// </summary>
        long TimeRemaining { get; }

        long BytesRemaining { get; }

        bool IsCompleted { get; }

        decimal Percentage { get; }
    }
}