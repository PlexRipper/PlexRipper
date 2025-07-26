using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexMediaQualityDTO : PlexMediaSlimQualityDTO
{
    /// <summary>
    /// The media id such as <see cref="PlexMovie"/> or <see cref="PlexTvShowEpisode"/>.
    /// </summary>
    public int MediaId { get; init; }

    /// <summary>
    /// The media data id such as <see cref="PlexMovieMediaData"/> or <see cref="PlexTvShowEpisodeMediaData"/>.
    /// </summary>
    public int DataId { get; init; }
}
