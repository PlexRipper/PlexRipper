namespace PlexRipper.Domain;

public class PlexTvShowEpisodeMediaData : BasePlexMediaData
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required ICollection<PlexTvShowEpisodeMediaDataPart> Parts { get; set; }

    public required int PlexTvShowEpisodeId { get; set; }

    public PlexTvShowEpisode? PlexTvShowEpisode { get; set; }
}
