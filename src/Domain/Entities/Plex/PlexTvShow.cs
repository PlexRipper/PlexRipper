namespace PlexRipper.Domain;

public class PlexTvShow : PlexMedia
{
    public override PlexMediaType Type => PlexMediaType.TvShow;

    #region Relationships

    public List<PlexTvShowGenre> PlexTvShowGenres { get; init; } = [];

    public List<PlexTvShowRole> PlexTvShowRoles { get; init; } = [];

    public List<PlexTvShowSeason> Seasons { get; set; } = [];

    #endregion
}
