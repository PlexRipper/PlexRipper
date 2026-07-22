namespace Reaparr.Domain;

/// <summary>
/// The comparison state projected onto a media item for the UI.
/// Determines which badges/chips to show when browsing.
/// </summary>
public enum PlexMediaComparisonState
{
    /// <summary>
    /// No current comparison scope for this media's library pair.
    /// Show the muted "not compared yet" chip; never show missing/HQ badges.
    /// </summary>
    NotCompared = 0,

    /// <summary>
    /// Current scope exists; this media matched at least one owned target at same-or-better quality.
    /// </summary>
    Owned = 1,

    /// <summary>
    /// Current scope exists; this media has no hit in any current owned library (anti-join result).
    /// </summary>
    Missing = 2,

    /// <summary>
    /// Current scope exists; a remote match is higher quality than the owned match(es).
    /// </summary>
    HigherQuality = 3,

    /// <summary>
    /// When comparison work is queued or processing and no current scope is available yet.
    /// </summary>
    Pending = 4,

    /// <summary>
    /// Current scope exists; the top-level media item is owned, but child media such as seasons or episodes are missing.
    /// </summary>
    Partial = 5,

    /// <summary>
    /// Current scope exists; the top-level media item is owned, but child media are both missing and available at higher quality.
    /// </summary>
    PartialAndHigherQuality = 6,
}