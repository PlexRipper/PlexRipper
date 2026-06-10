namespace Reaparr.Data;

/// <summary>
/// Identifies the metadata portion of a media cache snapshot.
/// </summary>
internal sealed class MediaQueryMetadataKey : IEquatable<MediaQueryMetadataKey>
{
    private readonly int[] _libraryIds;

    /// <summary>
    /// Creates a metadata key for a media type, normalized library scope, and cache-affecting visibility filters.
    /// </summary>
    public MediaQueryMetadataKey(
        PlexMediaType mediaType,
        IEnumerable<int> libraryIds,
        bool filterOfflineMedia,
        bool filterOwnedMedia)
    {
        MediaType = mediaType;
        _libraryIds = libraryIds.Distinct().OrderBy(x => x).ToArray();
        LibraryIds = Array.AsReadOnly(_libraryIds);
        FilterOfflineMedia = filterOfflineMedia;
        FilterOwnedMedia = filterOwnedMedia;
    }

    /// <summary>
    /// Gets the media type requested by the overview query.
    /// </summary>
    public PlexMediaType MediaType { get; }

    /// <summary>
    /// Gets the normalized distinct ascending library ids included in this key.
    /// </summary>
    public IReadOnlyList<int> LibraryIds { get; }

    /// <summary>
    /// Gets whether offline Plex server libraries were excluded when the key was created.
    /// </summary>
    public bool FilterOfflineMedia { get; }

    /// <summary>
    /// Gets whether owned Plex libraries were excluded when the key was created.
    /// </summary>
    public bool FilterOwnedMedia { get; }

    /// <summary>
    /// Returns whether this key contains any library affected by an invalidation request.
    /// </summary>
    public bool ContainsAnyLibrary(IReadOnlySet<int> libraryIds) => _libraryIds.Any(libraryIds.Contains);

    /// <summary>
    /// Compares keys using media type, normalized library id contents, and visibility filters.
    /// </summary>
    public bool Equals(MediaQueryMetadataKey? other) =>
        other is not null
        && MediaType == other.MediaType
        && _libraryIds.SequenceEqual(other._libraryIds)
        && FilterOfflineMedia == other.FilterOfflineMedia
        && FilterOwnedMedia == other.FilterOwnedMedia;

    /// <summary>
    /// Compares an arbitrary object with this metadata key.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as MediaQueryMetadataKey);

    /// <summary>
    /// Builds a hash code from the same normalized fields used by equality.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MediaType);
        foreach (var libraryId in _libraryIds)
            hash.Add(libraryId);
        hash.Add(FilterOfflineMedia);
        hash.Add(FilterOwnedMedia);
        return hash.ToHashCode();
    }
}