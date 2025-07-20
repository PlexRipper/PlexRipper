using PlexRipper.Domain;

namespace Application.Contracts;

public record DownloadPreviewDTO
{
    public required int Id { get; set; }

    public required string Title { get; set; } = string.Empty;

    public required long Size { get; set; }

    public required int ChildCount { get; set; }

    public required PlexMediaType MediaType { get; set; }

    public required List<DownloadPreviewDTO> Children { get; set; } = [];
}
