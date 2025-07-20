using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexMediaQualityDTO
{
    /// <summary>
    /// The media id such as <see cref="PlexMovie"/> or <see cref="PlexTvShowEpisode"/>.
    /// </summary>
    public required int MediaId { get; init; }

    /// <summary>
    /// The media data id such as <see cref="PlexMovieMediaData"/> or <see cref="PlexTvShowEpisodeMediaData"/>.
    /// </summary>
    public required int DataId { get; init; }

    /// <summary>
    ///  The type of media data, such as <see cref="PlexMediaType.Movie"/> or <see cref="PlexMediaType.Episode"/>.
    /// </summary>
    public required PlexMediaType MediaDataType { get; set; }

    /// <summary>
    /// The quality of the media data, such as <see cref="VideoQuality.DVD"/> or <see cref="VideoQuality.HD"/>.
    /// </summary>
    public required VideoQuality Quality { get; init; }
}
