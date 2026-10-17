namespace Reaparr.Domain;

public class DownloadTaskPhotoImageFileLog : DownloadTaskLogBase
{
    public required Guid DownloadTaskFileId { get; init; }

    public DownloadTaskPhotoImageFile? DownloadTaskFile { get; init; }
}
