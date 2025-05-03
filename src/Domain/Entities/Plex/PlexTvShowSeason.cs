using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexTvShowSeason : BasePlexMedia
{
    /// <summary>
    /// The Plex key of the <see cref="PlexTvShow"/> this belongs too.
    /// </summary>
    public required int ParentKey { get; set; }

    /// <summary>
    /// The Guid of the <see cref="PlexTvShow"/> this belongs too.
    /// </summary>
    public required string? ParentGuid { get; set; }

    #region Relationships

    public PlexTvShow? TvShow { get; set; }

    public int TvShowId { get; set; }

    public List<PlexTvShowEpisode> Episodes { get; set; } = [];

    #endregion

    #region Helpers

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Season;

    #endregion
}
