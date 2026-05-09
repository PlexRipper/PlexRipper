namespace Reaparr.Data.Contracts;

public record PagedMediaQueryResult
{
    public required int TotalCount { get; init; }
    
    public required int MediaCount { get; init; }

    public required int MovieCount { get; init; }

    public required int TvShowCount { get; init; }

    public required int SeasonCount { get; init; }

    public required int EpisodeCount { get; init; }

    public required long MediaSize { get; init; }
    
    public required List<PlexMediaSlimDTO> Items { get; init; }

}
