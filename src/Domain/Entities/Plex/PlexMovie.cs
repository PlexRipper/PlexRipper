namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : BasePlexMedia
{
    public required ICollection<PlexActor> Actors { get; set; } = [];

    public required ICollection<PlexGenre> Genres { get; set; } = [];

    public required ICollection<PlexCountry> Countries { get; set; } = [];

    public ICollection<PlexMovieMediaData> MediaDataList { get; set; } = [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
