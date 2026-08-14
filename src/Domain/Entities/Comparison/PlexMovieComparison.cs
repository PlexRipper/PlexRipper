namespace Reaparr.Domain;

/// <summary>
/// Stores a movie comparison hit where a remote movie matched an owned movie for a remote/owned library pair.
/// </summary>
/// <remarks>
/// A row means the remote movie was found in the owned library. Missing movies are not stored here; they are derived
/// by checking for remote movies with no hit row in a current <see cref="PlexComparisonState"/> scope.
/// <see cref="HitState"/> distinguishes an ordinary match from an upgrade candidate where the remote copy has higher quality.
/// </remarks>
public class PlexMovieComparison : BaseEntity
{
    /// <summary>
    /// Remote library that supplied the movie being compared.
    /// </summary>
    public required int RemotePlexLibraryId { get; init; }

    /// <summary>
    /// Owned library that contains the matched movie.
    /// </summary>
    public required int OwnedPlexLibraryId { get; init; }

    /// <summary>
    /// Remote movie media id being compared.
    /// </summary>
    public required int RemotePlexMediaId { get; init; }

    /// <summary>
    /// Owned movie media id that matched the remote movie.
    /// </summary>
    public required int OwnedPlexMediaId { get; init; }

    /// <summary>
    /// Stored hit outcome: <see cref="PlexMediaComparisonHitState.Matched"/> means owned quality is equal or better;
    /// <see cref="PlexMediaComparisonHitState.HigherQuality"/> means the remote movie is an upgrade candidate.
    /// </summary>
    public required PlexMediaComparisonHitState HitState { get; set; }

    /// <summary>
    /// Snapshot of the remote movie's quality at compare time.
    /// </summary>
    public required VideoQuality RemoteQuality { get; init; }

    /// <summary>
    /// Snapshot of the owned movie's quality at compare time.
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
