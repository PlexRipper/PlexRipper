using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public record DownloadMediaDTO
{
    public required List<int> MediaIds { get; init; } = [];

    public required List<PlexMediaQualityDTO> Qualities { get; init; } = [];

    public required PlexMediaType Type { get; init; }

    public required int PlexServerId { get; init; }

    public required int PlexLibraryId { get; init; }
}
