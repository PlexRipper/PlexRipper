namespace PlexRipper.Domain;

public class FileMergeProgress
{
    /// <summary>
    /// This is equal to the <see cref="DownloadFileTask"/> Id.
    /// </summary>

    public int Id { get; set; }

    /// <summary>
    /// This is equal to the <see cref="DownloadTask"/> Id the <see cref="DownloadFileTask"/> is currently handling.
    /// </summary>

    public int DownloadTaskId { get; set; }

    public long DataTransferred { get; set; }

    public long DataTotal { get; set; }

    public decimal Percentage => DataFormat.GetPercentage(DataTransferred, DataTotal);

    /// <summary>
    /// The transfer speed in bytes per second.
    /// </summary>
    public int TransferSpeed { get; set; }

    /// <summary>
    /// The time remaining in seconds the <see cref="DownloadFileTask"/> to finish.
    /// </summary>
    public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, TransferSpeed);

    public long BytesRemaining => DataTotal - DataTransferred;
}