namespace Reaparr.Domain;

public class DownloadTaskPhotoImageFile : DownloadTaskFileBase
{
    public required Guid ParentId { get; init; }

    public DownloadTaskPhotoFolder? Parent { get; init; }

    public PhotoAssetKind AssetKind { get; init; } = PhotoAssetKind.Image;

    public List<DownloadTaskPhotoImageFileLog> Logs { get; init; } = [];

    [NotMapped]
    public override PlexMediaType MediaType => PlexMediaType.PhotoImage;

    [NotMapped]
    public override DownloadTaskType DownloadTaskType => DownloadTaskType.PhotoImage;

    [NotMapped]
    public override bool IsDownloadable => true;

    [NotMapped]
    public override int Count => 1;

    public override DownloadTaskKey ToParentKey() =>
        new()
        {
            Type = DownloadTaskType.PhotoFolder,
            Id = ParentId,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };
}
