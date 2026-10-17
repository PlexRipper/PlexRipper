namespace Reaparr.Application.Contracts;

public class PlexPhotoBreadcrumbDTO
{
    public required int PlexPhotoFolderId { get; init; }

    public required string Title { get; init; }
}

public class PlexPhotoFolderPageDTO
{
    /// <summary>
    /// The configured source-root folder ID. A source root is itself a PlexPhotoFolder row.
    /// </summary>
    public required int SourceRootId { get; init; }

    /// <summary>
    /// The folder whose immediate children and direct files are shown.
    /// </summary>
    public required int FolderId { get; init; }

    public required List<PlexPhotoBreadcrumbDTO> Breadcrumbs { get; init; }

    public required PlexPhotoBrowsePageDTO Page { get; init; }
}
