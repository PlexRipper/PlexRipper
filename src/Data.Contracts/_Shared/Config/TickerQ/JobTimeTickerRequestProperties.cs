namespace Reaparr.Data.Contracts;

/// <summary>
/// Strongly typed, query-only properties mirrored from a TickerQ request.
/// The populated properties depend on the ticker's <see cref="JobTimeTicker.JobType"/>.
/// </summary>
public sealed record JobTimeTickerRequestProperties
{
    /// <summary>
    /// Used when querying <see cref="JobTypes.LibraryComparisonJob"/>
    /// </summary>
    public int OwnedPlexLibraryId { get; init; }

    /// <summary>
    /// Used when querying <see cref="JobTypes.LibraryComparisonJob"/>
    /// </summary>
    public int RemotePlexLibraryId { get; init; }
}