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
    /// Current scope exists; this media matched an owned target at same-or-better quality.
    /// </summary>
    Owned = 1,

    /// <summary>
    /// Current scope exists; this media is missing from owned libraries (anti-join result).
    /// </summary>
    Missing = 2,

    /// <summary>
    /// Current scope exists; a remote match is higher quality than the owned match(es).
    /// </summary>
    HigherQuality = 3,

    /// <summary>
    /// Current scope exists; the media is both missing at one level and higher-quality at another
    /// (e.g. a TV show with some missing seasons and some higher-quality episodes).
    /// </summary>
    MissingAndHigherQuality = 4,
}
