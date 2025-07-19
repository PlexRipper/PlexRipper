namespace PlexRipper.Domain;

public record PlexMediaQuality
{
    public required int Id { get; init; }

    public required PlexMediaType Type { get; init; }

    public required VideoQuality Quality { get; init; }
}
