namespace Reaparr.Domain;

/// <summary>
/// Stores the completed comparison scope for one remote library, one owned library, and one media type.
/// </summary>
/// <remarks>
/// This table answers whether hit rows are meaningful for a library pair. Without a scope row, missing hit rows mean
/// "not compared yet" rather than "missing". Content freshness is tracked once per library by <see cref="PlexLibrary"/>.
/// </remarks>
public class PlexComparisonState : BaseEntity
{
    /// <summary>
    /// Remote library source that was compared.
    /// </summary>
    public required int RemotePlexLibraryId { get; init; }

    /// <summary>
    /// Owned library target that was compared against the remote library.
    /// </summary>
    public required int OwnedPlexLibraryId { get; init; }

    /// <summary>
    /// Media type covered by this scope, such as Movie or TvShow.
    /// </summary>
    public required PlexMediaType MediaType { get; init; }

    /// <summary>
    /// UTC timestamp when this comparison scope completed successfully.
    /// </summary>
    public required DateTime CompletedAt { get; set; }

    /// <summary>
    /// Navigation to the remote library source.
    /// </summary>
    public PlexLibrary? RemotePlexLibrary { get; init; }

    /// <summary>
    /// Navigation to the owned library target.
    /// </summary>
    public PlexLibrary? OwnedPlexLibrary { get; init; }
}
