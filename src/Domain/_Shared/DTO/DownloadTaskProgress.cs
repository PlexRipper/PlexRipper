namespace Reaparr.Domain;

public record DownloadTaskProgress : IDownloadTaskProgress
{
    /// <summary>
    /// Gets the total number of bytes expected for the download.
    /// </summary>
    public long DataTotal { get; init; }

    /// <summary>
    /// Gets the overall download completion percentage.
    /// Percentage is from 0 to 100, with 2 decimal places
    /// </summary>
    public decimal Percentage { get; init; }

    /// <summary>
    /// Gets the number of bytes that have been downloaded so far.
    /// </summary>
    public long DataReceived { get; init; }

    /// <summary>
    /// Gets the current download speed in bytes per second.
    /// </summary>
    public long DownloadSpeed { get; init; }
}
