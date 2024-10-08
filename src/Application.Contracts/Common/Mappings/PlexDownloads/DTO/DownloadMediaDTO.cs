using PlexRipper.Domain;

namespace Application.Contracts;

public class DownloadMediaDTO
{
    public required List<int> MediaIds { get; init; } = [];

    public required PlexMediaType Type { get; init; }

    public required int PlexServerId { get; init; }

    public required int PlexLibraryId { get; init; }
}