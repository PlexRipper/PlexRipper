using PlexRipper.Domain;

namespace Application.Contracts;

/// <summary>
/// This is meant to be a slim version of <see cref="PlexMediaQualityDTO"/> that only contains the media type and quality. For high-level media containers such as TV-Shows, Seasons and Movies, this DTO is used to represent the quality of the media data contained within.
/// </summary>
public record PlexMediaSlimQualityDTO
{
    /// <summary>
    ///  The type of media data, such as <see cref="PlexMediaType.Movie"/> or <see cref="PlexMediaType.Episode"/>.
    /// </summary>
    public required PlexMediaType MediaDataType { get; set; }

    /// <summary>
    /// The quality of the media data, such as <see cref="VideoQuality.DVD"/> or <see cref="VideoQuality.HD"/>.
    /// </summary>
    public required VideoQuality Quality { get; init; }
}
