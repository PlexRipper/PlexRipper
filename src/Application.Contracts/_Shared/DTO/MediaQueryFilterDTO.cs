using System.ComponentModel;

namespace Reaparr.Application.Contracts;

public record MediaQueryFilterDTO
{
    #region FlexQueryParameters

    /// <summary>The JQL-lite query string.</summary>
    [QueryParam, BindFrom("query")]
    public string? Query { get; init; }

    /// <summary>The filter expression (DSL or JSON).</summary>
    [QueryParam, BindFrom("filter")]
    public string? Filter { get; init; }

    /// <summary>The sorting expression (e.g., "Name:asc,Age:desc").</summary>
    [QueryParam, BindFrom("sort")]
    public string? Sort { get; init; }

    /// <summary>The comma-separated list of fields to select.</summary>
    [QueryParam, BindFrom("select")]
    public string? Select { get; init; }

    /// <summary>The comma-separated list of fields to include.</summary>
    [QueryParam, BindFrom("includes")]
    public string? Includes { get; init; }

    /// <summary>The comma-separated list of fields to group by.</summary>
    [QueryParam, BindFrom("groupBy")]
    public string? GroupBy { get; init; }

    /// <summary>The HAVING clause for grouped queries.</summary>
    [QueryParam, BindFrom("having")]
    public string? Having { get; init; }

    /// <summary>The page number (1-indexed).</summary>
    [QueryParam, BindFrom("page")]
    public int? Page { get; init; }

    /// <summary>The number of items per page.</summary>
    [QueryParam, BindFrom("pageSize")]
    public int? PageSize { get; init; }

    /// <summary>Whether to include the total count in the result.</summary>
    [QueryParam, BindFrom("includeCount")]
    public bool? IncludeCount { get; init; }

    /// <summary>Whether to apply a DISTINCT clause.</summary>
    [QueryParam, BindFrom("distinct")]
    public bool? Distinct { get; init; }

    /// <summary>The projection mode (Flat, FlatMixed, Nested).</summary>
    [QueryParam, BindFrom("mode")]
    public string? Mode { get; init; }

    #endregion

    /// <summary>
    /// Is > 0 when a specific <see cref="PlexLibrary"/> is requested, and 0 when all are requested.
    /// </summary>
    [QueryParam, BindFrom("plexLibraryId")]
    public required int PlexLibraryId { get; init; }

    [QueryParam, BindFrom("filterOfflineMedia")]
    [DefaultValue(false)]
    public bool FilterOfflineMedia { get; init; }

    [QueryParam, BindFrom("filterOwnedMedia")]
    [DefaultValue(false)]
    public bool FilterOwnedMedia { get; init; }

    [QueryParam, BindFrom("mediaType")]
    [DefaultValue(false)]
    public required PlexMediaType MediaType { get; init; }
}