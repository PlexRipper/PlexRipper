namespace PlexRipper.Domain;

public record PlexMediaData
{
    public string MediaFormat { get; init; } = string.Empty;
    public long Duration { get; init; }
    public string VideoResolution { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int Bitrate { get; init; }
    public string VideoCodec { get; init; } = string.Empty;
    public string VideoFrameRate { get; init; } = string.Empty;
    public double AspectRatio { get; init; }
    public string VideoProfile { get; init; } = string.Empty;
    public string AudioProfile { get; init; } = string.Empty;
    public string AudioCodec { get; init; } = string.Empty;
    public int AudioChannels { get; init; }
    public List<PlexMediaDataPart> Parts { get; init; } = [];
}
