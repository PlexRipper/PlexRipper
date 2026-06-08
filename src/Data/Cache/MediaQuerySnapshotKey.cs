namespace Reaparr.Data;

public sealed class MediaQuerySnapshotKey : IEquatable<MediaQuerySnapshotKey>
{
    private readonly int[] _libraryIds;
    private readonly string _libraryIdsKey;

    public MediaQuerySnapshotKey(
        PlexMediaType mediaType,
        IEnumerable<int> libraryIds,
        string query,
        string filter,
        string sort,
        string select,
        string includes,
        string groupBy,
        string having,
        bool? includeCount,
        bool? distinct,
        string mode,
        bool filterOfflineMedia,
        bool filterOwnedMedia)
    {
        MediaType = mediaType;
        _libraryIds = libraryIds.Distinct().OrderBy(x => x).ToArray();
        LibraryIds = Array.AsReadOnly(_libraryIds);
        _libraryIdsKey = string.Join(',', _libraryIds);
        Query = query;
        Filter = filter;
        Sort = sort;
        Select = select;
        Includes = includes;
        GroupBy = groupBy;
        Having = having;
        IncludeCount = includeCount;
        Distinct = distinct;
        Mode = mode;
        FilterOfflineMedia = filterOfflineMedia;
        FilterOwnedMedia = filterOwnedMedia;
    }

    public PlexMediaType MediaType { get; }
    public IReadOnlyList<int> LibraryIds { get; }
    public string Query { get; }
    public string Filter { get; }
    public string Sort { get; }
    public string Select { get; }
    public string Includes { get; }
    public string GroupBy { get; }
    public string Having { get; }
    public bool? IncludeCount { get; }
    public bool? Distinct { get; }
    public string Mode { get; }
    public bool FilterOfflineMedia { get; }
    public bool FilterOwnedMedia { get; }

    public bool Equals(MediaQuerySnapshotKey? other) =>
        other is not null
        && MediaType == other.MediaType
        && _libraryIdsKey == other._libraryIdsKey
        && Query == other.Query
        && Filter == other.Filter
        && Sort == other.Sort
        && Select == other.Select
        && Includes == other.Includes
        && GroupBy == other.GroupBy
        && Having == other.Having
        && IncludeCount == other.IncludeCount
        && Distinct == other.Distinct
        && Mode == other.Mode
        && FilterOfflineMedia == other.FilterOfflineMedia
        && FilterOwnedMedia == other.FilterOwnedMedia;

    public override bool Equals(object? obj) => Equals(obj as MediaQuerySnapshotKey);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MediaType);
        hash.Add(_libraryIdsKey, StringComparer.Ordinal);
        hash.Add(Query, StringComparer.Ordinal);
        hash.Add(Filter, StringComparer.Ordinal);
        hash.Add(Sort, StringComparer.Ordinal);
        hash.Add(Select, StringComparer.Ordinal);
        hash.Add(Includes, StringComparer.Ordinal);
        hash.Add(GroupBy, StringComparer.Ordinal);
        hash.Add(Having, StringComparer.Ordinal);
        hash.Add(IncludeCount);
        hash.Add(Distinct);
        hash.Add(Mode, StringComparer.Ordinal);
        hash.Add(FilterOfflineMedia);
        hash.Add(FilterOwnedMedia);
        return hash.ToHashCode();
    }
}
