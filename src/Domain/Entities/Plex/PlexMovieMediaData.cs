namespace Reaparr.Domain;

public class PlexMovieMediaData : BasePlexMediaData
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required ICollection<PlexMovieMediaDataPart> Parts { get; set; }

    public required int PlexMovieId { get; set; }

    public PlexMovie? PlexMovie { get; set; }

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
