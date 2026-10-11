namespace Reaparr.Application.Contracts;

public record PlexMediaDataDTO
{
    public required int Id { get; init; }

    public required int PlexApiMediaId { get; init; }

    public required int PlexApiPartId { get; init; }

    public required string FileName { get; init; }

    public required long Duration { get; init; }

    public required long Size { get; init; }

    public required VideoQuality VideoResolution { get; init; }

    public required string VideoCodec { get; init; }

    public required string AudioCodec { get; init; }

    public required AudioQuality AudioQuality { get; init; } = AudioQuality.None;

    public required int? AudioBitrateKbps { get; init; }

    public required int? AudioBitDepth { get; init; }

    public required int? AudioSampleRateHz { get; init; }

    public required int? AudioChannels { get; init; }

    public required string? AudioChannelLayout { get; init; }

    public required string? AudioProfile { get; init; }
}
