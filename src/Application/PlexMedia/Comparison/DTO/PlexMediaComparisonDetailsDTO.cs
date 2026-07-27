namespace Reaparr.Application;

public record PlexMediaComparisonDetailsDTO
{
    public required int PlexMediaId { get; init; }

    public required PlexMediaType Type { get; init; }

    public required PlexMediaComparisonState State { get; init; }

    public required List<PlexMediaComparisonDetailsRowDTO> Rows { get; init; }
}

public record PlexMediaComparisonDetailsRowDTO
{
    public required string Title { get; init; }

    public required int PlexMediaId { get; init; }

    public required PlexMediaType Type { get; init; }

    public required PlexMediaComparisonState State { get; init; }

    public VideoQuality RemoteQuality { get; init; }

    public VideoQuality OwnedQuality { get; init; }

    public required int PlexLibraryId { get; init; }

    public required int PlexServerId { get; init; }

    public required List<PlexMediaComparisonDetailsRowDTO> Children { get; init; }
}