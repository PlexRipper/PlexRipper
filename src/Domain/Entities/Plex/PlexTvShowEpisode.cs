using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexTvShowEpisode : PlexMedia
{
    /// <summary>
    /// The PlexKey of the <see cref="PlexTvShowSeason"/> this belongs too.
    /// </summary>
    public int ParentKey { get; set; }

    #region Relationships

    public PlexTvShow? TvShow { get; set; }

    public int TvShowId { get; set; }

    public PlexTvShowSeason? TvShowSeason { get; set; }

    public int TvShowSeasonId { get; set; }

    #endregion

    #region Helpers

    [NotMapped]
    public List<PlexMediaData> EpisodeData => MediaData.MediaData;

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Episode;

    #endregion
}
