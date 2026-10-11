namespace Reaparr.Application;

public record PlexMediaQuality
{
    /// <summary>
    ///  The type of media data, such as <see cref="PlexMediaType.Movie"/> or <see cref="PlexMediaType.Episode"/>.
    /// </summary>
    public required PlexMediaType MediaDataType { get; init; }

    /// <summary>
    /// The quality of the media data, such as <see cref="VideoQuality.DVD"/> or <see cref="VideoQuality.HD"/>.
    /// </summary>
    public required VideoQuality Quality { get; init; }

    /// <summary>
    /// The audio quality for music media. Non-music media uses <see cref="AudioQuality.None"/>.
    /// </summary>
    public AudioQuality AudioQuality { get; init; } = AudioQuality.None;

    public string? AudioCodec { get; init; }

    public int? AudioBitrateKbps { get; init; }

    public int? AudioBitDepth { get; init; }

    public int? AudioSampleRateHz { get; init; }

    public int? AudioChannels { get; init; }

    public string? AudioChannelLayout { get; init; }

    public string? AudioProfile { get; init; }

    /// <summary>
    /// The media id such as <see cref="PlexMovie"/> or <see cref="PlexTvShowEpisode"/>.
    /// </summary>
    public int MediaId { get; init; }

    /// <summary>
    /// The media data id such as <see cref="PlexMovieMediaData"/> or <see cref="PlexTvShowEpisodeMediaData"/>.
    /// </summary>
    public int DataId { get; init; }
}
