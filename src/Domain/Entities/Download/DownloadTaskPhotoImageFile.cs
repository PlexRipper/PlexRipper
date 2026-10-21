namespace Reaparr.Domain;

/// <summary>
/// The single transferable file task for an image or video clip in a photo library.
/// It belongs to a queue Photo Folder, not an album, and owns its transfer state and log entries.
/// </summary>
public class DownloadTaskPhotoImageFile : DownloadTaskFileBase
{
    /// <summary>
    /// ID of the queue Photo Folder containing this file task.
    /// </summary>
    public required Guid ParentId { get; init; }

    /// <summary>
    /// Navigation to the queue Photo Folder containing this file task.
    /// </summary>
    public DownloadTaskPhotoFolder? Parent { get; init; }

    /// <summary>
    /// Captured asset kind distinguishing an image transfer from a video-clip transfer.
    /// </summary>
    public PhotoAssetKind AssetKind { get; init; } = PhotoAssetKind.Image;

    /// <summary>
    /// Persisted log entries associated directly with this file task.
    /// </summary>
    public List<DownloadTaskPhotoImageFileLog> Logs { get; init; } = [];

    /// <summary>
    /// Non-persisted media-family discriminator for image and clip transfers.
    /// </summary>
    [NotMapped]
    public override PlexMediaType MediaType => PlexMediaType.PhotoImage;

    /// <summary>
    /// Non-persisted task discriminator shared by image and clip file tasks.
    /// </summary>
    [NotMapped]
    public override DownloadTaskType DownloadTaskType => DownloadTaskType.PhotoImage;

    /// <summary>
    /// True because this leaf task owns the actual file transfer.
    /// </summary>
    [NotMapped]
    public override bool IsDownloadable => true;

    /// <summary>
    /// One, because this task represents exactly one transferable file.
    /// </summary>
    [NotMapped]
    public override int Count => 1;

    /// <summary>
    /// Creates the queue key of the Photo Folder containing this file task.
    /// </summary>
    public override DownloadTaskKey ToParentKey() =>
        new()
        {
            Type = DownloadTaskType.PhotoFolder,
            Id = ParentId,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };
}
