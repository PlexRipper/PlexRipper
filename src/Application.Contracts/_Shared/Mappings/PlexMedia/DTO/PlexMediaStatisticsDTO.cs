namespace Reaparr.Application.Contracts;

public class PlexMediaStatisticsDTO
{
    /// <summary>
    /// Gets or sets the total number of media items matching the active query before paging.
    /// This value is used by the frontend virtualizer.
    /// </summary>
    public required int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the count of media items in this returned page/list.
    /// </summary>
    public required int MediaCount { get; set; }

    public required int MovieCount { get; set; }

    public required int TvShowCount { get; set; }

    public required int SeasonCount { get; set; }

    public required int EpisodeCount { get; set; }

    public required long MediaSize { get; set; }

    public required List<PlexMediaSlimDTO> MediaList { get; init; }
}
