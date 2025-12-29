namespace Reaparr.Domain;

public class PlexTvShowEpisodeMediaData : BasePlexMediaData
{
    public required int PlexTvShowEpisodeId { get; set; }

    public PlexTvShowEpisode? PlexTvShowEpisode { get; set; }

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Episode;
}
