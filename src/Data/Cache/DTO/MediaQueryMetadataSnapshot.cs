namespace Reaparr.Data;

/// <summary>
/// Stores the metadata that is shared by all sorted media-list snapshots for one media overview query scope.
/// </summary>
internal sealed record MediaQueryMetadataSnapshot
{
    /// <summary>
    /// Gets the key describing the media type, library set, and visibility filters used to build this metadata.
    /// </summary>
    public required MediaQueryMetadataKey Key { get; init; }

    /// <summary>
    /// Gets the distinct role ids available for the cached overview scope.
    /// </summary>
    public required IReadOnlyList<int> Roles { get; init; }

    /// <summary>
    /// Gets the distinct country ids available for the cached overview scope.
    /// </summary>
    public required IReadOnlyList<int> Countries { get; init; }

    /// <summary>
    /// Gets the distinct genre ids available for the cached overview scope.
    /// </summary>
    public required IReadOnlyList<int> Genres { get; init; }

    /// <summary>
    /// Gets the distinct quality ids available for the cached overview scope.
    /// </summary>
    public required IReadOnlyList<int> Qualities { get; init; }

    /// <summary>
    /// Gets the total number of items matching the cached overview scope.
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Gets the count of media items for the requested media type within the cached scope.
    /// </summary>
    public required int MediaCount { get; init; }

    /// <summary>
    /// Gets the movie count reported for the cached scope.
    /// </summary>
    public required int MovieCount { get; init; }

    /// <summary>
    /// Gets the TV show count reported for the cached scope.
    /// </summary>
    public required int TvShowCount { get; init; }

    /// <summary>
    /// Gets the season count reported for the cached scope.
    /// </summary>
    public required int SeasonCount { get; init; }

    /// <summary>
    /// Gets the episode count reported for the cached scope.
    /// </summary>
    public required int EpisodeCount { get; init; }

    /// <summary>
    /// Gets the total movie count across the cached library scope.
    /// </summary>
    public required int TotalMovieCount { get; init; }

    /// <summary>
    /// Gets the total TV show count across the cached library scope.
    /// </summary>
    public required int TotalTvShowCount { get; init; }

    /// <summary>
    /// Gets the total season count across the cached library scope.
    /// </summary>
    public required int TotalSeasonCount { get; init; }

    /// <summary>
    /// Gets the total episode count across the cached library scope.
    /// </summary>
    public required int TotalEpisodeCount { get; init; }

    /// <summary>
    /// Gets the media size for items matching the cached overview scope.
    /// </summary>
    public required long MediaSize { get; init; }

    /// <summary>
    /// Gets the total media size across the cached library scope.
    /// </summary>
    public required long TotalMediaSize { get; init; }

    /// <summary>
    /// Gets when this metadata snapshot was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
