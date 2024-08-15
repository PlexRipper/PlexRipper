using PlexRipper.Domain;

namespace Application.Contracts;

public class PlexMediaSearchResultDTO
{
    public required string Title { get; set; } = string.Empty;

    public required List<int> PlexServerId { get; set; }

    public required List<int> PlexLibraryId { get; set; }

    public required PlexMediaType Type { get; set; }

    public required string FullThumbUrl { get; set; } = string.Empty;

    public required int Year { get; set; }
}
