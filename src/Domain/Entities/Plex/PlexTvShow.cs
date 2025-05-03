namespace PlexRipper.Domain;

public class PlexTvShow : BasePlexMedia
{
    public override PlexMediaType Type => PlexMediaType.TvShow;

    public required int GrandChildCount { get; set; }

    public List<PlexTvShowSeason> Seasons { get; set; } = [];

    public required List<PlexRole> Roles { get; set; } = [];

    public required List<PlexGenre> Genres { get; set; } = [];

    public required List<PlexCountry> Countries { get; set; } = [];
}
