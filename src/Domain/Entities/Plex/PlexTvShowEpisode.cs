using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexTvShowEpisode : BasePlexMediaData
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

    public new ICollection<PlexTvShowEpisodeMediaData> MediaDataList { get; set; } = [];

    public PlexTvShow? TvShow { get; set; }

    public int TvShowId { get; set; }

    public PlexTvShowSeason? TvShowSeason { get; set; }

    public int TvShowSeasonId { get; set; }

    #endregion

    #region Helpers

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Episode;

    #endregion
}
