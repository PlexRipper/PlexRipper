using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models;

/// <summary>
/// Attributes:
///     TAG (str): 'Media'
///     server (:class:`~plexapi.server.PlexServer`): PlexServer object this is from.
///     initpath (str): Relative path requested when retrieving specified data.
///     video (str): Video this media belongs to.
///     aspectRatio (float): Aspect ratio of the video (ex: 2.35).
///     audioChannels (int): Number of audio channels for this video (ex: 6).
///     audioCodec (str): Audio codec used within the video (ex: ac3).
///     bitrate (int): Bitrate of the video (ex: 1624)
///     container (str): Container this video is in (ex: avi).
///     duration (int): Length of the video in milliseconds (ex: 6990483).
///     height (int): Height of the video in pixels (ex: 256).
///     id (int): Plex ID of this media item (ex: 46184).
///     has64bitOffsets (bool): True if video has 64 bit offsets (?).
///     optimizedForStreaming (bool): True if video is optimized for streaming.
///     target (str): Media version target name.
///     title (str): Media version title.
///     videoCodec (str): Video codec used within the video (ex: ac3).
///     videoFrameRate (str): Video frame rate (ex: 24p).
///     videoResolution (str): Video resolution (ex: sd).
///     videoProfile (str): Video profile (ex: high).
///     width (int): Width of the video in pixels (ex: 608).
///     parts (list&lt;:class:`~plexapi.media.MediaPart`&gt;): List of MediaParts in this video.
/// </summary>
public class Medium
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("aspectRatio")]
    public double AspectRatio { get; set; }

    [JsonPropertyName("audioProfile")]
    public string AudioProfile { get; set; }

    [JsonPropertyName("videoProfile")]
    public string VideoProfile { get; set; }

    [JsonPropertyName("audioChannels")]
    public int AudioChannels { get; set; }

    [JsonPropertyName("audioCodec")]
    public string AudioCodec { get; set; }

    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; }

    [JsonPropertyName("container")]
    public string Container { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("optimizedForStreaming")]
    public int OptimizedForStreaming { get; set; } // TODO Convert to boolean at some point.

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("videoCodec")]
    public string VideoCodec { get; set; }

    [JsonPropertyName("videoFrameRate")]
    public string VideoFrameRate { get; set; }

    [JsonPropertyName("videoResolution")]
    public string VideoResolution { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("selected")]
    public bool Selected { get; set; }

    [JsonPropertyName("Part")]
    public Part[] Part { get; set; }
}