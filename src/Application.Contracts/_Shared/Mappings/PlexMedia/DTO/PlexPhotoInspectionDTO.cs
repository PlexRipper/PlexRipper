namespace Reaparr.Application.Contracts;

/// <summary>Bounded metadata for the photo asset inspection dialog.</summary>
public sealed record PhotoAssetInspectionDTO
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
    public required string Container { get; init; }
    public required long Size { get; init; }
    public required int? Width { get; init; }
    public required int? Height { get; init; }
    public required int? Duration { get; init; }
    public required string? VideoCodec { get; init; }
    public required string? AudioCodec { get; init; }
    public required string? Summary { get; init; }
    public required DateTime AddedAt { get; init; }
    public required DateTime? UpdatedAt { get; init; }
    public required DateTime? OriginallyAvailableAt { get; init; }
    public required string? RelativeFolderPath { get; init; }
    public required string? SourceRootTitle { get; init; }
    public required List<PhotoFolderBreadcrumbDTO> Breadcrumbs { get; init; } = [];
    public required PhotoAlbumSummaryPageDTO Albums { get; init; }
}
