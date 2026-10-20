namespace Reaparr.Application.Contracts;

/// <summary>A page of photo albums visible in the local catalog.</summary>
public sealed record PhotoAlbumPageDTO
{
    public required List<PhotoAlbumBrowseDTO> Items { get; init; } = [];
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required long TotalAssetBytes { get; init; }
}

/// <summary>A photo album card with ownership and unique member totals.</summary>
public sealed record PhotoAlbumBrowseDTO
{
    public required int Id { get; init; }
    public required int PlexServerId { get; init; }
    public required int PlexLibraryId { get; init; }
    public required string PlexServerName { get; init; }
    public required string PlexLibraryTitle { get; init; }
    public required string Title { get; init; }
    public required bool HasThumbnail { get; init; }
    public required int PlexApiRatingKey { get; init; }
    public required int PlexApiMetaDataKey { get; init; }
    public required int AssetCount { get; init; }
    public required long AssetBytes { get; init; }
}

/// <summary>A page of unique photo assets visible in the local catalog.</summary>
public sealed record PhotoAssetPageDTO
{
    public required List<PhotoAssetBrowseDTO> Items { get; init; } = [];
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required long TotalBytes { get; init; }
}

/// <summary>A photo asset card. Physical paths and transfer keys are intentionally omitted.</summary>
public sealed record PhotoAssetBrowseDTO
{
    public required int Id { get; init; }
    public required int PlexServerId { get; init; }
    public required int PlexLibraryId { get; init; }
    public required string PlexServerName { get; init; }
    public required string PlexLibraryTitle { get; init; }
    public required string Title { get; init; }
    public required PhotoAssetKind AssetKind { get; init; }
    public required bool HasThumbnail { get; init; }
    public required int PlexApiRatingKey { get; init; }
    public required int PlexApiMetaDataKey { get; init; }
    public required string OriginalFilename { get; init; }
    public required long Size { get; init; }
    public required string Container { get; init; }
    public required int? Width { get; init; }
    public required int? Height { get; init; }
    public required int? Duration { get; init; }
    public required string? RelativeFolderPath { get; init; }
    public required string? SourceRootTitle { get; init; }
    public required PhotoDownloadUnavailableReason? DownloadUnavailableReason { get; init; }
}

/// <summary>A page of physical folders. Asset totals are carried by each folder item and are recursive.</summary>
public sealed record PhotoFolderPageDTO
{
    public required List<PhotoFolderBrowseDTO> Items { get; init; } = [];
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
}

/// <summary>A physical source root or descendant folder card.</summary>
public sealed record PhotoFolderBrowseDTO
{
    public required int Id { get; init; }
    public required int PlexServerId { get; init; }
    public required int PlexLibraryId { get; init; }
    public required string Title { get; init; }
    public required string RelativePath { get; init; }
    public required int? ParentId { get; init; }
    public required bool IsRoot { get; init; }
    public required bool? IsActive { get; init; }
    public required int RecursiveAssetCount { get; init; }
    public required long RecursiveAssetBytes { get; init; }
}

/// <summary>A complete root-to-current breadcrumb item without source filesystem paths.</summary>
public sealed record PhotoFolderBreadcrumbDTO
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string RelativePath { get; init; }
}

/// <summary>Independent folder and direct-asset pages for one physical folder.</summary>
public sealed record PhotoFolderChildrenDTO
{
    public required PhotoFolderBrowseDTO Folder { get; init; }
    public required List<PhotoFolderBreadcrumbDTO> Breadcrumbs { get; init; } = [];
    public required PhotoFolderPageDTO Folders { get; init; }
    public required PhotoAssetPageDTO Assets { get; init; }
}

/// <summary>A bounded page of album summaries for an asset inspection dialog.</summary>
public sealed record PhotoAlbumSummaryPageDTO
{
    public required List<PhotoAlbumSummaryDTO> Items { get; init; } = [];
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
}

/// <summary>A compact album membership summary.</summary>
public sealed record PhotoAlbumSummaryDTO
{
    public required int Id { get; init; }
    public required int PlexServerId { get; init; }
    public required int PlexLibraryId { get; init; }
    public required string PlexServerName { get; init; }
    public required string PlexLibraryTitle { get; init; }
    public required string Title { get; init; }
    public required bool HasThumbnail { get; init; }
    public required int PlexApiRatingKey { get; init; }
    public required int PlexApiMetaDataKey { get; init; }
}
