namespace Reaparr.Domain;

public class PlexTvShowEpisodeMediaDataPart : BasePlexMediaDataPart
{
    public required int PlexTvShowEpisodeId { get; set; }

    public PlexTvShowEpisode? PlexTvShowEpisode { get; set; }

    public required int PlexTvShowEpisodeMediaDataId { get; set; }

    public PlexTvShowEpisodeMediaData? PlexTvShowEpisodeMediaData { get; set; }
}
