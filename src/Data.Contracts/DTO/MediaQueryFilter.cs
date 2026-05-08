using FlexQuery.NET.Models;

namespace Reaparr.Data.Contracts;

public record MediaQueryFilter
{
    public required PlexMediaType MediaType { get; init; }

    /// <summary>
    /// Is > 0 when a specific <see cref="PlexLibrary"/> is requested, and 0 when all are requested.
    /// </summary>
    public required int PlexLibraryId { get; init; }

    public required bool FilterOfflineMedia { get; init; }

    public required bool FilterOwnedMedia { get; init; }

    public required FlexQueryParameters Parameters { get; init; }
}
