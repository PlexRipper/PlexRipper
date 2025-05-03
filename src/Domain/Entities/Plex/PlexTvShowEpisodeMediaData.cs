namespace PlexRipper.Domain;

public class PlexTvShowEpisodeMediaData : BasePlexMediaDataRow
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required List<LibraryMediaItemPartDTO> Parts { get; set; }
}
