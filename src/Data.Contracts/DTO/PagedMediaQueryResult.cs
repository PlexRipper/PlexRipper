namespace Reaparr.Data.Contracts;

public record PagedMediaQueryResult
{
    public int TotalCount { get; set; }
    
    public int MediaCount { get; set; }

    public int MovieCount { get; set; }

    public int TvShowCount { get; set; }

    public int SeasonCount { get; set; }

    public int EpisodeCount { get; set; }

    public long MediaSize { get; set; }

    public List<PlexMediaSlimDTO> Items { get; set; } = [];

}
