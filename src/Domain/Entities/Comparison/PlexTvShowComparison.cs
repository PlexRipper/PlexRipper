namespace Reaparr.Domain;

/// <summary>
/// A comparison hit row for a single remote TV show matched against an owned library/media item.
/// One row per (remote show, owned library/target) pair within a comparison scope.
/// Show-level rows can be derived from episode comparison rows or materialized directly.
/// </summary>
public class PlexTvShowComparison : BaseEntity
{
    public required int RemotePlexLibraryId { get; init; }
    public required int OwnedPlexLibraryId { get; init; }
    public required int RemotePlexMediaId { get; init; }
    public required int OwnedPlexMediaId { get; init; }
    public required PlexMediaComparisonHitState HitState { get; set; }
    public required VideoQuality RemoteQuality { get; init; }
    public required VideoQuality OwnedQuality { get; init; }
    public required PlexMediaComparisonMatchType MatchType { get; init; }
    public required DateTime ComparedAt { get; init; }
    public required int AlgorithmVersion { get; init; }
}
