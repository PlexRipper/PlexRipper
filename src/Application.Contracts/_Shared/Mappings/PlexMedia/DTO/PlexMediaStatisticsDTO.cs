namespace Reaparr.Application.Contracts;

public class PlexMediaStatisticsDTO
{
    public required int MediaCount { get; set; }

    public required int MovieCount { get; set; }

    public required int TvShowCount { get; set; }

    public required int SeasonCount { get; set; }

    public required int EpisodeCount { get; set; }

    public required long MediaSize { get; set; }

    public required List<PlexMediaSlimDTO> MediaList { get; init; }
}
