namespace Reaparr.Domain;

/// <summary>
/// A persisted download log entry owned directly by an image or clip file task, without an album-level log owner.
/// </summary>
public class DownloadTaskPhotoImageFileLog : DownloadTaskLogBase
{
    /// <summary>
    /// ID of the photo image or clip file task that owns this log entry.
    /// </summary>
    public required Guid DownloadTaskFileId { get; init; }

    /// <summary>
    /// Navigation to the file task that owns this log entry.
    /// </summary>
    public DownloadTaskPhotoImageFile? DownloadTaskFile { get; init; }
}
