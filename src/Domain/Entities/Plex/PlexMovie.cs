namespace PlexRipper.Domain;

[Table("PlexMovie")]
public class PlexMovie : BasePlexMedia
{
    public required ICollection<PlexActor> Actors { get; set; } = [];

    public required ICollection<PlexGenre> Genres { get; set; } = [];

    public required ICollection<PlexCountry> Countries { get; set; } = [];

    public ICollection<PlexMovieMediaData> MediaDataList { get; set; } = [];

    [NotMapped]
    public List<PlexMediaQuality> Qualities
    {
        get
        {
            return MediaDataList
                .Select(y => new PlexMediaQuality(y.Quality))
                .Reverse() // This sorts from lowest to highest quality
                .TakeLast(1) // TODO:remove this when quality selector for downloading is implemented
                .ToList();
        }
    }

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Movie;
}
