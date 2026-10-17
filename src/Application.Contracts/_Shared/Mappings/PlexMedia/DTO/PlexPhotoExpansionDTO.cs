namespace Reaparr.Application.Contracts;

public class PlexPhotoSelectionDTO
{
    public required PhotoItemKind Kind { get; init; }

    public required int Id { get; init; }
}

public class PlexPhotoAssetIdentityDTO
{
    public required int PlexPhotoMediaDataId { get; init; }

    public required int PlexLibraryId { get; init; }

    public required int PlexServerId { get; init; }

    public required int PlexApiRatingKey { get; init; }

    public required PhotoAssetKind AssetKind { get; init; }
}

public class PlexPhotoExpansionDTO
{
    public required List<PlexPhotoAssetIdentityDTO> Assets { get; init; }
}
