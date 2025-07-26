namespace PlexRipper.Domain;

public class PlexTvShowEpisode : BasePlexMedia
{
    /// <summary>
    /// The PlexKey of the <see cref="PlexTvShowSeason"/> this belongs too.
    /// </summary>
    public int ParentKey { get; set; }

    /// <summary>
    /// The Guid of the <see cref="PlexTvShowSeason"/> this belongs too.
    /// </summary>
    public required string? ParentGuid { get; set; }

    #region Relationships

    public PlexTvShow? TvShow { get; set; }

    public int TvShowId { get; set; }

    public PlexTvShowSeason? TvShowSeason { get; set; }

    public int TvShowSeasonId { get; set; }

    /// <summary>
    /// Gets or sets the list of media data for this episode.
    /// </summary>
    public ICollection<PlexTvShowEpisodeMediaData> MediaDataList { get; init; } = [];

    /// <summary>
    /// Gets or sets the list of qualities available for this episode.
    /// </summary>
    public required ICollection<PlexMediaQuality> Qualities { get; set; } = [];

    public ICollection<PlexTvShowEpisodeMediaQuality> TvShowEpisodeMediaQualities { get; set; } = [];

    #endregion

    #region Helpers

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Episode;

    #endregion
}
