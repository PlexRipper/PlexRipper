using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public record PlexQualityDTO
{
    public required string Name { get; init; }
    public required int Count { get; init; }

    public required VideoQuality Quality { get; init; }
}
