namespace PlexRipper.Domain;

public class PlexTvShow : PlexMedia
{
    public override PlexMediaType Type => PlexMediaType.TvShow;

    #region Relationships

    public List<PlexTvShowGenre> PlexTvShowGenres { get; set; } = new();

    public List<PlexTvShowRole> PlexTvShowRoles { get; set; } = new();

    public List<PlexTvShowSeason> Seasons { get; set; } = new();

    #endregion
}
