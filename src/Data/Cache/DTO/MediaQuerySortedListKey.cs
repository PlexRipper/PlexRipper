namespace Reaparr.Data;

/// <summary>
/// Identifies the cached sorted media list for one metadata key and one canonical ascending sort field.
/// </summary>
internal sealed class MediaQuerySortedListKey : IEquatable<MediaQuerySortedListKey>
{
    /// <summary>
    /// Creates a sorted-list key for a shared metadata scope and stored ascending sort field.
    /// </summary>
    public MediaQuerySortedListKey(MediaQueryMetadataKey metadataKey, string normalizedAscendingSortField)
    {
        MetadataKey = metadataKey;
        NormalizedAscendingSortField = normalizedAscendingSortField;
    }

    /// <summary>
    /// Gets the metadata key shared by all sorted-list snapshots in the same query scope.
    /// </summary>
    public MediaQueryMetadataKey MetadataKey { get; }

    /// <summary>
    /// Gets the canonical ascending field used to build and store the sorted snapshot.
    /// </summary>
    public string NormalizedAscendingSortField { get; }

    /// <summary>
    /// Gets the normalized library ids through the shared metadata key.
    /// </summary>
    public IReadOnlyList<int> LibraryIds => MetadataKey.LibraryIds;

    /// <summary>
    /// Returns whether this sorted-list key depends on any library affected by an invalidation request.
    /// </summary>
    public bool ContainsAnyLibrary(IReadOnlySet<int> libraryIds) => MetadataKey.ContainsAnyLibrary(libraryIds);

    /// <summary>
    /// Compares sorted-list keys by metadata key and canonical ascending sort field.
    /// </summary>
    public bool Equals(MediaQuerySortedListKey? other) =>
        other is not null
        && MetadataKey.Equals(other.MetadataKey)
        && NormalizedAscendingSortField == other.NormalizedAscendingSortField;

    /// <summary>
    /// Compares an arbitrary object with this sorted-list key.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as MediaQuerySortedListKey);

    /// <summary>
    /// Builds a hash code from the same metadata key and sort field used by equality.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MetadataKey);
        hash.Add(NormalizedAscendingSortField, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}
