using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexTvShowEpisodeMediaQuality
{
    public PlexTvShowEpisodeMediaQuality() { }

    [SetsRequiredMembers]
    public PlexTvShowEpisodeMediaQuality(int plexMediaQualityId, int plexLibraryId, int plexTvShowEpisodeId)
    {
        PlexMediaQualityId = plexMediaQualityId;
        PlexTvShowEpisodeId = plexTvShowEpisodeId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public required int PlexMediaQualityId { get; set; }

    [Column(Order = 2)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public required int PlexTvShowEpisodeId { get; set; }
}
