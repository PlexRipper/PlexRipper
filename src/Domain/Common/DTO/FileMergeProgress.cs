namespace PlexRipper.Domain;

/// <summary>
/// This is used to track the progress of a <see cref="FileTask"/>.
/// </summary>
public class FileMergeProgress
{
    /// <summary>
    /// This is equal to the <see cref="FileTask"/> Id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// This is equal to the <see cref="DownloadTaskGeneric"/> Id the <see cref="FileTask"/> is currently handling.
    /// </summary>
    public Guid DownloadTaskId { get; init; }

    public DownloadTaskType DownloadTaskType { get; set; }

    public long DataTransferred { get; init; }

    public long DataTotal { get; init; }

    public decimal Percentage => DataFormat.GetPercentage(DataTransferred, DataTotal);

    /// <summary>
    /// The transfer speed in bytes per second.
    /// </summary>
    public int TransferSpeed { get; init; }

    /// <summary>
    /// The time remaining in seconds the <see cref="FileTask"/> to finish.
    /// </summary>
    public long TimeRemaining => DataFormat.GetTimeRemaining(BytesRemaining, TransferSpeed);

    public long BytesRemaining => DataTotal - DataTransferred;

    /// <summary>
    /// Gets or sets the <see cref="PlexServer"/> Id the <see cref="FileTask"/> is currently handling.
    /// Note: This is needed in the front-end to update the correct DownloadTask.
    /// </summary>
    public int PlexServerId { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="PlexLibrary"/> Id the <see cref="FileTask"/> is currently handling.
    /// Note: This is needed in the front-end to update the correct DownloadTask.
    /// </summary>
    public int PlexLibraryId { get; init; }

    public DownloadTaskKey ToKey() =>
        new()
        {
            Type = DownloadTaskType,
            Id = DownloadTaskId,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };

    public override string ToString() =>
        $"[FileMergeProgress {DownloadTaskId} - {Percentage}% - {DataFormat.FormatSpeedString(TransferSpeed)} - {DataFormat.FormatSizeString(BytesRemaining)} / {DataFormat.FormatSizeString(DataTotal)} - {DataFormat.FormatTimeSpanString(TimeSpan.FromSeconds(TimeRemaining))}]";
}
