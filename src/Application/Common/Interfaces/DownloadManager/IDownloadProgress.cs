using System.Collections.Generic;

namespace PlexRipper.Application.Common
{
    public interface IDownloadProgress
    {
        int Id { get; set; }
        decimal Percentage { get; }
        int DownloadSpeed { get; }
        long DataReceived { get; }
        long DataTotal { get; set; }

        /// <summary>
        /// The download time remaining in seconds
        /// </summary>
        long TimeRemaining { get; }

        List<IDownloadWorkerProgress> WorkerProgresses { get; }
    }
}