using Reaparr.Application.Contracts;

namespace Reaparr.Data;

/// <summary>
/// Stores a full sorted media-list snapshot that can be sliced into individual requested pages.
/// </summary>
internal sealed record MediaQuerySortedListSnapshot
{
    /// <summary>
    /// Gets the key describing the shared metadata scope and stored ascending sort field.
    /// </summary>
    public required MediaQuerySortedListKey Key { get; init; }

    /// <summary>
    /// Gets all media items for the cached scope in the stored ascending sort order.
    /// </summary>
    public required IReadOnlyList<PlexMediaSlimDTO> Items { get; init; }

    /// <summary>
    /// Gets the navigation index list built for the full stored ascending item list.
    /// </summary>
    public required IReadOnlyList<MediaNavigationIndexDTO> NavigationIndexes { get; init; }

    /// <summary>
    /// Gets when this sorted-list snapshot was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
