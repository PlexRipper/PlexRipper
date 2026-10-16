namespace Reaparr.Domain;

public class PlexMovieMediaData : BasePlexMediaData
{
    public required int PlexMovieId { get; set; }

    public PlexMovie? PlexMovie { get; set; }

    public ICollection<PlexMovieDataStream> Streams { get; init; } = [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
