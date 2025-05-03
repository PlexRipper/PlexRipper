namespace PlexRipper.Domain;

public class PlexTvShow : BasePlexMedia
{
    public override PlexMediaType Type => PlexMediaType.TvShow;

    public required int GrandChildCount { get; set; }

    #region Relationships

    public ICollection<PlexTvShowSeason> Seasons { get; set; } = [];

    public required ICollection<PlexRole> Roles { get; set; } = [];

    public required ICollection<PlexGenre> Genres { get; set; } = [];

    public required ICollection<PlexCountry> Countries { get; set; } = [];

    #endregion
}
