namespace Reaparr.Domain;

/// <summary>
/// The outcome of a single remote→owned comparison hit row.
/// Distinct from <see cref="PlexMediaComparisonState"/> which is the media-side browse state.
/// </summary>
public enum PlexMediaComparisonHitState
{
    /// <summary>
    /// Remote matches owned at same-or-better quality (no upgrade needed).
    /// </summary>
    Matched = 0,

    /// <summary>
    /// Remote quality is strictly higher than the owned quality (upgrade candidate).
    /// </summary>
    HigherQuality = 1,
}
