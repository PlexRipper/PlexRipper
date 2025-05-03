namespace PlexRipper.Domain;

public class PlexTvShowEpisodeMediaData : BasePlexMediaData
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required List<PlexTvShowEpisodeMediaDataPart> Parts { get; set; }
}
