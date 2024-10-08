namespace PlexRipper.Domain;

public class PlexTvShow : PlexMedia
{
    public override PlexMediaType Type => PlexMediaType.TvShow;

    #region Relationships

    public List<PlexTvShowSeason> Seasons { get; set; } = [];

    #endregion
}
