namespace PlexRipper.Domain;

public class PlexMovieMediaQuality
{
    [Column(Order = 1)]
    public required int PlexMediaQualityId { get; set; }

    [Column(Order = 2)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public required int PlexMovieId { get; set; }

    [Column(Order = 4)]
    public required int PlexMovieMediaDataId { get; set; }

    #region Navigation Properties

    public PlexMediaQuality? PlexMediaQuality { get; set; }

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexMovie? PlexMovie { get; set; }

    public PlexMovieMediaData? PlexMovieMediaData { get; set; }

    #endregion

    [NotMapped]
    public PlexMediaType Type => PlexMediaType.Movie;
}
