namespace Reaparr.Domain;

/// <summary>
/// A comparison hit row for a single remote season matched against an owned library/media item.
/// Season-level rows can be derived from episode comparison rows or materialized directly.
/// </summary>
public class PlexSeasonComparison : BaseEntity
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
}
