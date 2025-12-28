namespace Reaparr.Domain;

public class PlexMovieMediaDataPart : BasePlexMediaDataPart
{
    public required int PlexMovieId { get; set; }

    public PlexMovie? PlexMovie { get; set; }

    public required int PlexMovieMediaDataId { get; set; }

    public PlexMovieMediaData? PlexMovieMediaData { get; set; }
}
