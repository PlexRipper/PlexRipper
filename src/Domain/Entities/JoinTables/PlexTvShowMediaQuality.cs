namespace PlexRipper.Domain;

public class PlexTvShowMediaQuality : BasePlexMediaQuality
{
    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexTvShowId { get; set; }

    #region Navigation Properties

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexTvShow? PlexTvShow { get; set; }

    #endregion

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.TvShow;
}
