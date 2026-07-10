namespace Reaparr.Data;

/// <summary>
/// Normalizes supported Plex media sort expressions into a canonical cache key and resolves title-sort
/// field selection based on the number of libraries in scope.
/// </summary>
internal static class MediaSortNormalizer
{
    private const string DefaultTitleSortMultiLibrary = nameof(BasePlexMedia.SearchTitle);
    private const string DefaultTitleSortSingleLibrary = "sortIndex";

    /// <summary>
    /// All allowed sort fields (case-insensitive). The normalization function resolves aliases to
    /// canonical handler field names.
    /// </summary>
    private static readonly HashSet<string> _titleFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "sortTitle",
        "title",
        "sortIndex",
        nameof(BasePlexMedia.SortIndex),
        nameof(BasePlexMedia.SearchTitle),
    };

    /// <summary>
    /// Maps a requested sort field (case-insensitive) to its canonical handler field name.
    /// </summary>
    private static readonly Dictionary<string, string> _fieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["year"] = nameof(BasePlexMedia.Year),
        ["addedAt"] = nameof(BasePlexMedia.AddedAt),
        ["updatedAt"] = nameof(BasePlexMedia.UpdatedAt),
        ["duration"] = nameof(BasePlexMedia.Duration),
        ["mediaSize"] = nameof(BasePlexMedia.MediaSize),
        ["quality"] = "quality",
        // Canonical names also map to themselves
        [nameof(BasePlexMedia.Year)] = nameof(BasePlexMedia.Year),
        [nameof(BasePlexMedia.AddedAt)] = nameof(BasePlexMedia.AddedAt),
        [nameof(BasePlexMedia.UpdatedAt)] = nameof(BasePlexMedia.UpdatedAt),
        [nameof(BasePlexMedia.Duration)] = nameof(BasePlexMedia.Duration),
        [nameof(BasePlexMedia.MediaSize)] = nameof(BasePlexMedia.MediaSize),
    };

    /// <summary>
    /// Result returned when sort normalization succeeds — the canonical field and whether the view
    /// should be served in descending order.
    /// </summary>
    public sealed record Result(string Field, bool Descending);

    /// <summary>
    /// Normalizes a requested sort expression. Returns null for unsupported expressions (caller
    /// should bypass the cache and forward to the live query handler).
    /// </summary>
    public static Result? Normalize(this string? sort, int libraryCount)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            var titleField = libraryCount == 1
                ? DefaultTitleSortSingleLibrary
                : DefaultTitleSortMultiLibrary;

            return new Result(titleField, false);
        }

        var segments = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 1)
            return null;

        var parts = segments[0].Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length is < 1 or > 2)
            return null;

        var field = NormalizeField(parts[0], libraryCount);
        if (field is null)
            return null;

        var direction = parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1])
            ? parts[1]
            : "asc";

        var descending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        if (!descending && !direction.Equals("asc", StringComparison.OrdinalIgnoreCase))
            return null;

        return new Result(field, descending);
    }

    /// <summary>
    /// Resolves a requested field name to its canonical handler field. Title fields switch between
    /// "sortIndex" (single library) and "SearchTitle" (multi-library).
    /// </summary>
    private static string? NormalizeField(this string requested, int libraryCount)
    {
        if (_titleFields.Contains(requested))
            return libraryCount == 1
                ? DefaultTitleSortSingleLibrary
                : DefaultTitleSortMultiLibrary;

        return _fieldMap.TryGetValue(requested, out var canonical)
            ? canonical
            : null;
    }
}
