namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : BasePlexMedia
{
    public required ICollection<PlexActor> Actors { get; set; } = [];

    public required ICollection<PlexGenre> Genres { get; set; } = [];

    public required ICollection<PlexCountry> Countries { get; set; } = [];

    public ICollection<PlexMovieMediaData> MediaDataList { get; set; } = [];

    public ICollection<PlexMediaQuality> Qualities { get; set; } = [];

    // Navigation property to access join table data with media data relationship
    public ICollection<PlexMovieMediaQuality> MovieMediaQualities { get; set; } = [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
