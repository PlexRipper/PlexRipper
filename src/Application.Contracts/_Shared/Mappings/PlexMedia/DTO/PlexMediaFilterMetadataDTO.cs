namespace Reaparr.Application.Contracts;

/// <summary>
/// Lightweight filter metadata containing only available IDs for roles, countries, genres, and qualities.
/// Served lazily when the filter dropdown is opened.
/// </summary>
public record PlexMediaFilterMetadataDTO
{
    public required List<int> Roles { get; init; }
    public required List<int> Countries { get; init; }
    public required List<int> Genres { get; init; }
    public required List<int> Qualities { get; init; }
}
