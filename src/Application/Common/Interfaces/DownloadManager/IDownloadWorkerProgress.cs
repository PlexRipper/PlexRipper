namespace PlexRipper.Application.Common
{
    public interface IDownloadWorkerProgress
    {
        int Id { get; }

        long DataReceived { get; }

        long DataTotal { get; }

        int DownloadSpeed { get; }

        string DownloadSpeedFormatted { get; }

        /// <summary>
        /// The time remaining in seconds for this DownloadWorker to finish.
        /// </summary>
        long TimeRemaining { get; }

        long BytesRemaining { get; }

        bool IsCompleted { get; }

        decimal Percentage { get; }

        int DownloadSpeedAverage { get; }
    }
}