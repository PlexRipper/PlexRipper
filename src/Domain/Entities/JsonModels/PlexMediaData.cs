namespace PlexRipper.Domain;

public record PlexMediaData
{
    public string MediaFormat { get; init; }
    public long Duration { get; init; }
    public string VideoResolution { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Bitrate { get; init; }
    public string VideoCodec { get; init; }
    public string VideoFrameRate { get; init; }
    public double AspectRatio { get; init; }
    public string VideoProfile { get; init; }
    public string AudioProfile { get; init; }
    public string AudioCodec { get; init; }
    public int AudioChannels { get; init; }
    public bool OptimizedForStreaming { get; init; }
    public string Protocol { get; init; }
    public bool Selected { get; init; }
    public List<PlexMediaDataPart> Parts { get; init; }

    public bool IsMultiPart => Parts.Count > 1;
}
