namespace PlexRipper.Domain;

public class PlexTvShowEpisodeMediaDataPart : BasePlexMediaDataPart
{
    /// <summary>
    /// An array of parts for this media item.
    /// </summary>
    public required ICollection<PlexTvShowEpisodeMediaDataStream> Streams { get; set; }

    public required int PlexTvShowEpisodeId { get; set; }

    public PlexTvShowEpisode? PlexTvShowEpisode { get; set; }

    public required int PlexTvShowEpisodeMediaDataId { get; set; }

    public PlexTvShowEpisodeMediaData? PlexTvShowEpisodeMediaData { get; set; }
}
