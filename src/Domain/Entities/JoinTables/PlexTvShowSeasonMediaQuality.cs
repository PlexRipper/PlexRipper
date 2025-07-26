namespace PlexRipper.Domain;

public class PlexTvShowSeasonMediaQuality
{
    [Column(Order = 1)]
    public required int PlexMediaQualityId { get; set; }

    [Column(Order = 2)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public required int PlexTvShowSeasonId { get; set; }
}
