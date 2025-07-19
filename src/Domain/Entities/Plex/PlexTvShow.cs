namespace PlexRipper.Domain;

public class PlexTvShow : BasePlexMedia
{
    public override PlexMediaType Type => PlexMediaType.TvShow;

    public required int GrandChildCount { get; set; }

    #region Relationships

    public ICollection<PlexTvShowSeason> Seasons { get; set; } = [];

    public required ICollection<PlexActor> Actors { get; init; } = [];

    public required ICollection<PlexGenre> Genres { get; init; } = [];

    public required ICollection<PlexCountry> Countries { get; init; } = [];

    #endregion

    [NotMapped]
    public List<PlexMediaQuality> Qualities =>
        Seasons.SelectMany(x => x.Qualities).Distinct().OrderBy(q => q.Quality).ToList();
}
