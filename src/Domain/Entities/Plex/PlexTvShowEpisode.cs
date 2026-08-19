namespace Reaparr.Domain;

public class PlexTvShowEpisode : BasePlexMedia
{
    public required int EpisodeNumber { get; set; }

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

    #endregion

    #region Helpers

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Episode;

    /// <summary>
    /// Gets or sets the expected season number for this episode. This property is not mapped to the database and is used for internal calculations or display purposes.
    /// This is used when the episode is not linked to a season with a season number, but we still want to know what season it is expected to be in.
    /// </summary>
    [NotMapped]
    public int ExpectedSeasonNumber { get; set; } = -1;

    #endregion
}
