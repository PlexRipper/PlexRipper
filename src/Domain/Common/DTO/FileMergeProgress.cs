namespace PlexRipper.Domain;

/// <summary>
/// This is used to track the progress of a <see cref="DownloadFileTask"/>.
/// </summary>
public class FileMergeProgress
{
    /// <summary>
    /// This is equal to the <see cref="DownloadFileTask"/> Id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// This is equal to the <see cref="DownloadTaskGeneric"/> Id the <see cref="DownloadFileTask"/> is currently handling.
    /// </summary>
    public Guid DownloadTaskId { get; set; }

    public DownloadTaskType DownloadTaskType { get; set; }

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

    /// <summary>
    /// Gets or sets the <see cref="PlexServer"/> Id the <see cref="DownloadFileTask"/> is currently handling.
    /// Note: This is needed in the front-end to update the correct DownloadTask.
    /// </summary>
    public int PlexServerId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PlexLibrary"/> Id the <see cref="DownloadFileTask"/> is currently handling.
    /// Note: This is needed in the front-end to update the correct DownloadTask.
    /// </summary>
    public int PlexLibraryId { get; set; }

    public DownloadTaskKey ToKey() => new(DownloadTaskType, DownloadTaskId, PlexServerId, PlexLibraryId);
}