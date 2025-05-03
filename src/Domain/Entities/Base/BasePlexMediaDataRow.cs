namespace PlexRipper.Domain;

public class BasePlexMediaDataRow
{
    /// <summary>
    /// Unique media identifier.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Duration of the media in milliseconds.
    /// </summary>
    public required int Duration { get; set; }

    /// <summary>
    /// Bitrate in bits per second.
    /// </summary>
    public required int Bitrate { get; set; }

    /// <summary>
    /// Video width in pixels.
    /// </summary>
    public required int Width { get; set; }

    /// <summary>
    /// Video height in pixels.
    /// </summary>
    public required int Height { get; set; }

    /// <summary>
    /// Aspect ratio of the video.
    /// </summary>
    public required float AspectRatio { get; set; }

    /// <summary>
    /// Number of audio channels.
    /// </summary>
    public required int AudioChannels { get; set; }

    /// <summary>
    /// Audio codec used.
    /// </summary>
    public required string AudioCodec { get; set; }

    /// <summary>
    /// Video codec used.
    /// </summary>
    public required string VideoCodec { get; set; }

    /// <summary>
    /// Video resolution (e.g., 4k).
    /// </summary>
    public required string VideoResolution { get; set; }

    /// <summary>
    /// File container type.
    /// </summary>
    public required string Container { get; set; }

    /// <summary>
    /// Frame rate of the video (e.g., 24p).
    /// </summary>
    public required string VideoFrameRate { get; set; }

    /// <summary>
    /// Video profile (e.g., main 10).
    /// </summary>
    public required string VideoProfile { get; set; }

    /// <summary>
    /// Video profile (e.g., main 10).
    /// </summary>
    public required string AudioProfile { get; set; }

    /// <summary>
    /// Indicates whether voice activity is detected.
    /// </summary>
    public required bool HasVoiceActivity { get; set; }
}
