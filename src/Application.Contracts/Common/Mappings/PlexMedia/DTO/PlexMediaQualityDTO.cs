using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexMediaQualityDTO
{
    public required int Id { get; init; }
    public required VideoQuality Quality { get; init; }

    public required PlexMediaType Type { get; set; }
}
