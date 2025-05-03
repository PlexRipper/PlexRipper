namespace PlexRipper.Domain;

public class PlexMovieMediaDataPart : BasePlexMediaDataPart
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required ICollection<PlexMovieMediaDataStream> Streams { get; set; }

    public required int PlexMovieId { get; set; }

    public PlexMovie? PlexMovie { get; set; }

    public required int PlexMovieMediaDataId { get; set; }

    public PlexMovieMediaData? PlexMovieMediaData { get; set; }
}
