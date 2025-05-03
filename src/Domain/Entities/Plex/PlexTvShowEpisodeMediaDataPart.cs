namespace PlexRipper.Domain;

public class PlexTvShowEpisodeMediaDataPart : BasePlexMediaDataPart
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required ICollection<PlexTvShowEpisodeMediaDataStream> Streams { get; set; }
}
