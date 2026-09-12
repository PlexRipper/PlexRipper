namespace Reaparr.PublicAPI;

public sealed record TorznabRequest
{
    public required TorznabQueryType Type { get; init; }
    public required string Query { get; init; }
    public required int Season { get; init; }
    public required int Episode { get; init; }
    public required int TvdbId { get; init; }
    public required string ImdbId { get; init; }
    public required int TmdbId { get; init; }
    public required string ApiKey { get; init; }
    public required int Limit { get; init; }
    public required int Offset { get; init; }
    public required int[] Categories { get; init; }
    public int? Extended { get; init; }

    public bool IsRssSync =>
        string.IsNullOrWhiteSpace(Query)
        && string.IsNullOrWhiteSpace(ImdbId)
        && TmdbId <= 0
        && TvdbId <= 0
        && Season <= 0
        && Episode <= 0;

    public bool HasTvShowCategory => Categories.Length == 0 || Categories.Any(x => x is >= 5000 and < 6000);

    public bool HasMovieCategory => Categories.Length == 0 || Categories.Any(x => x is >= 2000 and < 3000);
}
