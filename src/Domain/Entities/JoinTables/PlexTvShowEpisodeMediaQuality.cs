namespace PlexRipper.Domain;

public class PlexTvShowEpisodeMediaQuality
{
    [Column(Order = 1)]
    public required int PlexMediaQualityId { get; set; }

    [Column(Order = 2)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public required int PlexTvShowEpisodeId { get; set; }

    [Column(Order = 4)]
    public required int PlexTvShowEpisodeMediaDataId { get; set; }

    #region Navigation Properties

    public PlexMediaQuality? PlexMediaQuality { get; set; }

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexTvShowEpisode? PlexTvShowEpisode { get; set; }

    public PlexTvShowEpisodeMediaData? PlexTvShowEpisodeMediaData { get; set; }

    #endregion
}
