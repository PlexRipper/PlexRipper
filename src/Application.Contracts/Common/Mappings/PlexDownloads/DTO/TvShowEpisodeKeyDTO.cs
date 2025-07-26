using PlexRipper.Domain;

namespace Application.Contracts;

public record TvShowEpisodeKeyDTO
{
    public required int TvShowId { get; init; }

    public required int SeasonId { get; init; }

    public required int EpisodeId { get; init; }

    public required List<PlexTvShowEpisodeMediaData> MediaDataList { get; init; }

    public List<PlexMediaQuality> Qualities =>
        MediaDataList.Select(y => y.ToPlexMediaQuality()).OrderBy(q => q.Quality).ToList().PickMediaQuality();
}
