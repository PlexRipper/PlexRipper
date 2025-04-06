namespace PlexRipper.Domain;

public class PlexTvShow : PlexMedia
{
    public override PlexMediaType Type => PlexMediaType.TvShow;

    public required int GrandChildCount { get; set; }

    public List<PlexTvShowSeason> Seasons { get; set; } = [];

    public List<PlexRole> Roles { get; set; } = [];

    public List<PlexGenre> Genres { get; set; } = [];

    public List<PlexCountry> Countries { get; set; } = [];
}
