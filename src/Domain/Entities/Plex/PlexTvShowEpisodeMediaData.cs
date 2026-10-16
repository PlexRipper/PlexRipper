namespace Reaparr.Domain;

public class PlexTvShowEpisodeMediaData : BasePlexMediaData
{
    public required int PlexTvShowEpisodeId { get; set; }

    public PlexTvShowEpisode? PlexTvShowEpisode { get; set; }

    public ICollection<PlexTvShowEpisodeDataStream> Streams { get; init; } = [];

    [NotMapped]
    public override PlexMediaType Type => PlexMediaType.Episode;
}
