using PlexRipper.Domain;

namespace Application.Contracts;

public class PlexMediaSlimDTO
{
    public required int Id { get; set; }

    public required int Index { get; set; }

    public required string Title { get; set; } = string.Empty;

    public required string SortTitle { get; set; } = string.Empty;

    public required int Year { get; set; }

    public required int Duration { get; set; }

    public required long MediaSize { get; set; }

    public required int ChildCount { get; set; }

    public required DateTime AddedAt { get; set; }

    public required DateTime UpdatedAt { get; set; }

    public required int PlexLibraryId { get; set; }

    public required int PlexServerId { get; set; }

    public required PlexMediaType Type { get; set; }

    public required bool HasThumb { get; set; }

    public required string FullThumbUrl { get; set; } = string.Empty;

    public required List<PlexMediaQualityDTO> Qualities { get; set; } = new();

    public List<PlexMediaSlimDTO> Children { get; set; } = new();
}
