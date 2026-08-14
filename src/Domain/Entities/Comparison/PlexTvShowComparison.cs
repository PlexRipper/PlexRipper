namespace Reaparr.Domain;

/// <summary>
/// Stores a TV-show comparison hit where a remote show matched an owned show for a remote/owned library pair.
/// </summary>
/// <remarks>
/// A row means the remote show was found in the owned library. Missing shows are not stored here; they are derived
/// by checking for remote shows with no hit row in a current <see cref="PlexComparisonState"/> scope.
/// <see cref="HitState"/> distinguishes an ordinary match from an upgrade candidate where the remote copy has higher quality.
/// </remarks>
public class PlexTvShowComparison : BaseEntity
{
    /// <summary>
    /// Remote library that supplied the show being compared.
    /// </summary>
    public required int RemotePlexLibraryId { get; init; }

    /// <summary>
    /// Owned library that contains the matched show.
    /// </summary>
    public required int OwnedPlexLibraryId { get; init; }

    /// <summary>
    /// Remote TV show media id being compared.
    /// </summary>
    public required int RemotePlexMediaId { get; init; }

    /// <summary>
    /// Owned TV show media id that matched the remote show.
    /// </summary>
    public required int OwnedPlexMediaId { get; init; }

    /// <summary>
    /// Stored hit outcome: <see cref="PlexMediaComparisonHitState.Matched"/> means owned quality is equal or better;
    /// <see cref="PlexMediaComparisonHitState.HigherQuality"/> means the remote show is an upgrade candidate.
    /// </summary>
    public required PlexMediaComparisonHitState HitState { get; set; }

    /// <summary>
    /// Snapshot of the remote show's quality at compare time.
    /// </summary>
    public required VideoQuality RemoteQuality { get; init; }

    /// <summary>
    /// Snapshot of the owned show's quality at compare time.
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
