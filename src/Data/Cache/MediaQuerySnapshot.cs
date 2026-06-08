using Reaparr.Application.Contracts;

namespace Reaparr.Data;

public sealed record MediaQuerySnapshot
{
    public required MediaQuerySnapshotKey Key { get; init; }
    public required IReadOnlyList<PlexMediaSlimDTO> Items { get; init; }
    public required IReadOnlyList<MediaNavigationIndexDTO> NavigationIndexes { get; init; }
    public required IReadOnlyList<int> Roles { get; init; }
    public required IReadOnlyList<int> Countries { get; init; }
    public required IReadOnlyList<int> Genres { get; init; }
    public required IReadOnlyList<int> Qualities { get; init; }
    public required int TotalCount { get; init; }
    public required int MediaCount { get; init; }
    public required int MovieCount { get; init; }
    public required int TvShowCount { get; init; }
    public required int SeasonCount { get; init; }
    public required int EpisodeCount { get; init; }
    public required int TotalMovieCount { get; init; }
    public required int TotalTvShowCount { get; init; }
    public required int TotalSeasonCount { get; init; }
    public required int TotalEpisodeCount { get; init; }
    public required long MediaSize { get; init; }
    public required long TotalMediaSize { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public PagedMediaQueryResult CreatePage(string queryHash, int page, int pageSize)
    {
        var normalizedPage = Math.Max(page, 1);
        var effectivePageSize = pageSize > 0 ? pageSize : Items.Count;
        var offset = effectivePageSize == 0 ? 0 : (normalizedPage - 1) * effectivePageSize;

        return new PagedMediaQueryResult
        {
            QueryHash = queryHash,
            Page = normalizedPage,
            PageSize = effectivePageSize,
            Items = Items.Skip(offset).Take(effectivePageSize).Select(CopyItem).ToList(),
            NavigationIndexes = NavigationIndexes.ToList(),
            Roles = Roles.ToList(),
            Countries = Countries.ToList(),
            Genres = Genres.ToList(),
            Qualities = Qualities.ToList(),
            TotalCount = TotalCount,
            MediaCount = MediaCount,
            MovieCount = MovieCount,
            TvShowCount = TvShowCount,
            SeasonCount = SeasonCount,
            EpisodeCount = EpisodeCount,
            TotalMovieCount = TotalMovieCount,
            TotalTvShowCount = TotalTvShowCount,
            TotalSeasonCount = TotalSeasonCount,
            TotalEpisodeCount = TotalEpisodeCount,
            MediaSize = MediaSize,
            TotalMediaSize = TotalMediaSize,
        };
    }

    private static PlexMediaSlimDTO CopyItem(PlexMediaSlimDTO item) =>
        item with { Qualities = item.Qualities.ToList() };
}
