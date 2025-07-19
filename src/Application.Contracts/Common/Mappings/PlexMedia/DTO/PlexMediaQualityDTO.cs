using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexMediaQualityDTO
{
    public required int Id { get; set; }
    public required VideoQuality Quality { get; init; }

    public required PlexMediaType Type { get; set; }
}
