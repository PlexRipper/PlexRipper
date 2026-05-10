namespace Reaparr.Application.Contracts;

public class PlexMediaStatisticsDTO
{
    public required int Page { get; set; }

    public required int PageSize { get; set; }

    public required int MediaCount { get; set; }

    public required int MovieCount { get; set; }

    public required int TvShowCount { get; set; }

    public required int SeasonCount { get; set; }

    public required int EpisodeCount { get; set; }

    public required long MediaSize { get; set; }

    public required List<PlexMediaSlimDTO> MediaList { get; init; }

    public required List<MediaNavigationIndexDTO> NavigationIndexes { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the list of distinct roles available in the media items of the <see cref="PlexLibrary"/>.
    /// </summary>
    public required List<int> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of distinct countries available in the media items of the <see cref="PlexLibrary"/>.
    /// </summary>
    public required List<int> Countries { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of distinct genres available in the media items of the <see cref="PlexLibrary"/>.
    /// </summary>
    public required List<int> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of distinct quality levels available in the media items of the <see cref="PlexLibrary"/>.
    /// </summary>
    public required List<int> Qualities { get; set; } = [];
}
