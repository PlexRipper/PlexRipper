namespace Reaparr.Domain;

public class DownloadTaskPhotoFolder : DownloadTaskParentBase
{
    public Guid? ParentId { get; set; }

    public DownloadTaskPhotoFolder? Parent { get; set; }

    public required ICollection<DownloadTaskPhotoFolder> Children { get; set; } = [];

    public required ICollection<DownloadTaskPhotoImageFile> Files { get; set; } = [];

    public required string RelativePath { get; set; }

    public required string DownloadRootPath { get; set; }

    public required string DestinationRootPath { get; set; }

    [NotMapped]
    public IEnumerable<DownloadTaskBase> ChildrenWithFiles => Children.Cast<DownloadTaskBase>().Concat(Files);

    [NotMapped]
    public override PlexMediaType MediaType => PlexMediaType.PhotoAlbum; // TODO : Should this be PhotoFolder?

    [NotMapped]
    public override DownloadTaskType DownloadTaskType => DownloadTaskType.PhotoFolder;

    [NotMapped]
    public override bool IsDownloadable => false;

    [NotMapped]
    public override int Count => Children.Sum(x => x.Count) + Files.Sum(x => x.Count) + 1;

    public override DownloadTaskKey? ToParentKey() =>
        ParentId is not Guid parentId
            ? null
            : new()
            {
                Type = DownloadTaskType.PhotoFolder,
                Id = parentId,
                PlexServerId = PlexServerId,
                PlexLibraryId = PlexLibraryId,
            };
}
