namespace PlexRipper.Domain;

public class PlexMediaQuality : BaseEntity
{
    /// <summary>
    /// The quality of the media data, such as <see cref="VideoQuality.HD"/> or <see cref="VideoQuality.QHD"/>.
    /// </summary>
    [Column(Order = 1)]
    public required VideoQuality Quality { get; init; }

    #region Relationships

    public ICollection<PlexMovie> Movies { get; set; } = [];

    public ICollection<PlexTvShow> TvShows { get; set; } = [];

    public ICollection<PlexTvShowSeason> TvShowSeasons { get; set; } = [];

    public ICollection<PlexTvShowEpisode> TvShowEpisodes { get; set; } = [];

    #endregion
}
