namespace PlexRipper.Domain;

public class PlexMovieMediaDataPart : BasePlexMediaDataPart
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required ICollection<PlexMovieMediaDataStream> Streams { get; set; }
}
