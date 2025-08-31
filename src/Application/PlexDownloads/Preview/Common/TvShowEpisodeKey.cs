namespace Reaparr.Application;

public record TvShowEpisodeKey
{
    public required int TvShowId { get; init; }

    public required int SeasonId { get; init; }

    public required int EpisodeId { get; init; }

    public required ICollection<PlexTvShowEpisodeMediaData> MediaDataList { get; init; }
}
