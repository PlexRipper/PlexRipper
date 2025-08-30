namespace Reaparr.Domain;

[Table("PlexMovie")]
public class PlexMovie : BasePlexMedia
{
    public required ICollection<PlexActor> Actors { get; init; } = [];

    public required ICollection<PlexGenre> Genres { get; init; } = [];

    public required ICollection<PlexCountry> Countries { get; init; } = [];

    public ICollection<PlexMovieMediaData> MediaDataList { get; init; } = [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
