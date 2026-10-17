namespace Reaparr.Application.Contracts;

public class PlexPhotoSourceRootDTO
{
    public required int PlexPhotoFolderId { get; init; }

    public required int PlexLibraryId { get; init; }

    public required int PlexServerId { get; init; }

    public required string Name { get; init; }
}
