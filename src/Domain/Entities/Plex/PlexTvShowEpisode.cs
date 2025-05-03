using System.ComponentModel.DataAnnotations.Schema;

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

    public ICollection<PlexTvShowEpisodeMediaData> MediaDataList { get; set; } = [];

    public PlexTvShow? TvShow { get; set; }

    public int TvShowId { get; set; }

    public PlexTvShowSeason? TvShowSeason { get; set; }

    public int TvShowSeasonId { get; set; }

    #endregion

    #region Helpers

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Episode;

    [NotMapped]
    public List<PlexMediaQuality> Qualities
    {
        get
        {
            return MediaDataList
                .Select(y => new PlexMediaQuality(y.VideoResolution))
                .Reverse() // This sorts from lowest to highest quality
                .TakeLast(1) // TODO:remove this when quality selector for downloading is implemented
                .ToList();
        }
    }

    #endregion
}
