using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public record PlexMediaDataDTO
{
    public required long Duration { get; init; }

    public required string VideoResolution { get; init; }

    public required string VideoCodec { get; init; }

    public required string AudioCodec { get; init; }
}
