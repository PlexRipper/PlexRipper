namespace Reaparr.Domain;

public class PlexMusicTrackComparison : BaseEntity
{
    public required int RemotePlexLibraryId { get; init; }
    public required int OwnedPlexLibraryId { get; init; }
    public required int RemotePlexMediaId { get; init; }
    public required int OwnedPlexMediaId { get; init; }

    /// <summary>
    /// Stored outcome of comparing the remote and owned audio representatives.
    /// Unknown or incomparable encodings remain matched rather than becoming upgrades.
    /// </summary>
    public PlexMediaComparisonHitState HitState { get; init; } = PlexMediaComparisonHitState.Matched;

    /// <summary>
    /// Audio classification of the representative remote file used for this hit.
    /// </summary>
    public AudioQuality RemoteAudioQuality { get; init; } = AudioQuality.Unknown;

    /// <summary>
    /// Audio classification of the representative owned file used for this hit.
    /// </summary>
    public AudioQuality OwnedAudioQuality { get; init; } = AudioQuality.Unknown;

    /// <summary>
    /// Persisted file IDs that explain the two audio classifications.
    /// </summary>
    public int? RemotePlexMediaDataId { get; init; }

    public int? OwnedPlexMediaDataId { get; init; }

    public required PlexMediaComparisonMatchType MatchType { get; init; }

    public required DateTime ComparedAt { get; init; }
}
