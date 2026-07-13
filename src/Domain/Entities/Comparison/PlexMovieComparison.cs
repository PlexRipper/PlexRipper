namespace Reaparr.Domain;

/// <summary>
/// A comparison hit row for a single remote movie matched against an owned library/media item.
/// One row per (remote movie, owned library/target) pair within a comparison scope.
/// </summary>
public class PlexMovieComparison : BaseEntity
{
    /// <summary>
    /// Remote library source.
    /// </summary>
    public required int RemotePlexLibraryId { get; init; }

    /// <summary>
    /// Owned library target.
    /// </summary>
    public required int OwnedPlexLibraryId { get; init; }

    /// <summary>
    /// Remote movie being compared.
    /// </summary>
    public required int RemotePlexMediaId { get; init; }

    /// <summary>
    /// Matched owned movie (the target that owns this copy).
    /// </summary>
    public required int OwnedPlexMediaId { get; init; }

    /// <summary>
    /// The hit outcome: Matched (no upgrade) or HigherQuality (upgrade candidate).
    /// </summary>
    public required PlexMediaComparisonHitState HitState { get; set; }

    /// <summary>
    /// Snapshot of the remote movie's <see cref="BasePlexMedia.Quality"/> at compare time.
    /// </summary>
    public required VideoQuality RemoteQuality { get; init; }

    /// <summary>
    /// Snapshot of the owned movie's <see cref="BasePlexMedia.Quality"/> at compare time.
    /// </summary>
    public required VideoQuality OwnedQuality { get; init; }

    /// <summary>
    /// The waterfall layer that produced the match.
    /// </summary>
    public required PlexMediaComparisonMatchType MatchType { get; init; }

    /// <summary>
    /// When this comparison row was calculated.
    /// </summary>
    public required DateTime ComparedAt { get; init; }

    /// <summary>
    /// The algorithm version at compare time; allows recalculation after matching logic changes.
    /// </summary>
    public required int AlgorithmVersion { get; init; }
}
