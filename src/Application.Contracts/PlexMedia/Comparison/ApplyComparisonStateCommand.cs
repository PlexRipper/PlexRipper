namespace Reaparr.Application.Contracts;

/// <summary>
/// Projects stored comparison scopes and hit rows onto <see cref="PlexMediaSlimDTO"/> items
/// for a single library, setting <see cref="PlexMediaSlimDTO.ComparisonState"/> per item in-place.
/// Annotates each item with comparison state derived from per-owned-target hit coverage.
/// </summary>
/// <param name="Items">The overview page items. Modified in-place.</param>
/// <param name="PlexLibraryId">The Plex library being browsed. 0 = all-library, skipped.</param>
/// <param name="MediaType">Movie or TvShow.</param>
public record ApplyComparisonStateCommand(
    List<PlexMediaSlimDTO> Items,
    int PlexLibraryId,
    PlexMediaType MediaType
) : ICommand<Result>;
