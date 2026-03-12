namespace Reaparr.Domain;

public interface IDownloadTaskProgress
{
    /// <summary>
    /// Gets the total number of bytes expected for the download.
    /// </summary>
    long DataTotal { get; set; }

    /// <summary>
    /// Gets the overall download completion percentage.
    /// Percentage is from 0 to 100, with 2 decimal places
    /// </summary>
    decimal Percentage { get; set; }

    /// <summary>
    /// Gets the number of bytes that have been downloaded so far.
    /// </summary>
    long DataReceived { get; set; }

    /// <summary>
    /// Gets the current download speed in bytes per second.
    /// </summary>
    long DownloadSpeed { get; set; }

    /// <summary>
    /// Gets the ETA, time remaining in seconds before completion
    /// </summary>
    int TimeRemaining { get; set; }
}
