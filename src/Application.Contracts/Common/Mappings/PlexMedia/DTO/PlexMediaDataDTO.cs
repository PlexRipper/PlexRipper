namespace Application.Contracts;

public record PlexMediaDataDTO
{
    public required string MediaFormat { get; init; }

    public required long Duration { get; init; }

    public required string VideoResolution { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public required int Bitrate { get; init; }

    public required string VideoCodec { get; init; }

    public required string VideoFrameRate { get; init; }

    public required double AspectRatio { get; init; }

    public required string VideoProfile { get; init; }

    public required string AudioProfile { get; init; }

    public required string AudioCodec { get; init; }

    public required int AudioChannels { get; init; }

    public required List<PlexMediaDataPartDTO> Parts { get; init; }
}
