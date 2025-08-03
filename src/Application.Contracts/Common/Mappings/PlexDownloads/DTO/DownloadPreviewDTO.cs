using PlexRipper.Domain;

namespace Application.Contracts;

public record DownloadPreviewContainerDTO
{
    public required long TotalSize { get; init; }

    public required Dictionary<string, bool> Expanded { get; init; }

    public required List<DownloadPreviewDTO> Previews { get; init; }
}

public record DownloadPreviewDTO
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public required long Size { get; init; }

    public required PlexMediaType Type { get; init; }

    public required List<DownloadPreviewDTO> Children { get; init; } = [];

    public required List<PlexMediaQualityDTO> Qualities { get; init; } = [];
}
