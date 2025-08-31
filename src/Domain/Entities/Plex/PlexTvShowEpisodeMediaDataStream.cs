namespace Reaparr.Domain;

public class PlexTvShowEpisodeMediaDataStream : BasePlexMediaDataStream
{
    public required int PlexTvShowEpisodeId { get; set; }

    public PlexTvShowEpisode? PlexTvShowEpisode { get; set; }

    public required int PlexTvShowEpisodeMediaDataId { get; set; }

    public PlexTvShowEpisodeMediaData? PlexTvShowEpisodeMediaData { get; set; }

    public required int PlexTvShowEpisodeMediaDataPartId { get; set; }

    public PlexTvShowEpisodeMediaDataPart? PlexTvShowEpisodeMediaDataPart { get; set; }
}
