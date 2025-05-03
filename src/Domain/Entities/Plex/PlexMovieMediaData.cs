namespace PlexRipper.Domain;

public class PlexMovieMediaData : BasePlexMediaDataRow
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required List<LibraryMediaItemPartDTO> Parts { get; set; }
}
