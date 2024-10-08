namespace Application.Contracts;

public record PlexMediaQualityDTO
{
    public required string Quality { get; init; }

    public required string DisplayQuality { get; init; }

    // TODO Pre-calculate this to avoid doing it every time, used to identify the different media files
    public required string HashId { get; init; }
}
