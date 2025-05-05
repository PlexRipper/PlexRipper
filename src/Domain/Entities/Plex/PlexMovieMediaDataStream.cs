namespace PlexRipper.Domain;

public class PlexMovieMediaDataStream : BasePlexMediaDataStream
{
    public required int PlexMovieId { get; set; }

    public PlexMovie? PlexMovie { get; set; }

    public required int PlexMovieMediaDataId { get; set; }

    public PlexMovieMediaData? PlexMovieMediaData { get; set; }

    public required int PlexMovieMediaDataPartId { get; set; }

    public PlexMovieMediaDataPart? PlexMovieMediaDataPart { get; set; }
}
