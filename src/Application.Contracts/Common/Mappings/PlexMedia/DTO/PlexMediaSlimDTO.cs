using PlexRipper.Domain;

namespace Application.Contracts;

public class PlexMediaSlimDTO
{
    public required int Id { get; init; }

    public int Index { get; set; }

    public required string Title { get; init; } = string.Empty;

    public required string SortTitle { get; init; } = string.Empty;

    public required int Year { get; init; }

    public required int Duration { get; init; }

    public required long MediaSize { get; init; }

    public required int ChildCount { get; init; }

    public required DateTime AddedAt { get; init; }

    public required DateTime? UpdatedAt { get; init; }

    public required int PlexLibraryId { get; init; }

    public required int PlexServerId { get; init; }

    public required PlexMediaType Type { get; init; }

    public required bool HasThumb { get; init; }

    public required string FullThumbUrl { get; init; } = string.Empty;

    public required List<PlexMediaQualityDTO> Qualities { get; init; } = new();

    public List<PlexMediaSlimDTO> Children { get; set; } = new();
}
