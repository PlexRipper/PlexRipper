namespace Reaparr.Domain;

/// <summary>
/// Stores a TV-episode comparison hit where a remote episode matched an owned episode for a remote/owned library pair.
/// </summary>
/// <remarks>
/// Episode rows are the leaf truth for TV comparison. A row means the remote episode was found in the owned library.
/// Missing episodes are derived by checking for remote episodes with no hit row in a current <see cref="PlexComparisonState"/>
/// scope. <see cref="HitState"/> distinguishes an ordinary match from an upgrade candidate where the remote episode has
/// higher quality.
/// </remarks>
public class PlexEpisodeComparison : BaseEntity
{
    /// <summary>
    /// Remote library that supplied the episode being compared.
    /// </summary>
    public required int RemotePlexLibraryId { get; init; }

    /// <summary>
    /// Owned library that contains the matched episode.
    /// </summary>
    public required int OwnedPlexLibraryId { get; init; }

    /// <summary>
    /// Remote TV episode media id being compared.
    /// </summary>
    public required int RemotePlexMediaId { get; init; }

    /// <summary>
    /// Owned TV episode media id that matched the remote episode.
    /// </summary>
    public required int OwnedPlexMediaId { get; init; }

    /// <summary>
    /// Stored hit outcome: <see cref="PlexMediaComparisonHitState.Matched"/> means owned quality is equal or better;
    /// <see cref="PlexMediaComparisonHitState.HigherQuality"/> means the remote episode is an upgrade candidate.
    /// </summary>
    public required PlexMediaComparisonHitState HitState { get; set; }

    /// <summary>
    /// Snapshot of the remote episode's quality at compare time.
    /// </summary>
    public required VideoQuality RemoteQuality { get; init; }

    /// <summary>
    /// Snapshot of the owned episode's quality at compare time.
    /// </summary>
    public required VideoQuality OwnedQuality { get; init; }

    /// <summary>
    /// Matching waterfall layer that produced this hit.
    /// </summary>
    public required PlexMediaComparisonMatchType MatchType { get; init; }

    /// <summary>
    /// UTC timestamp when this hit was calculated.
    /// </summary>
    public required DateTime ComparedAt { get; init; }
}