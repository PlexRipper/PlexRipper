using PlexRipper.Domain;

namespace Data.Contracts;

public record MediaQueryFilter
{
    public required PlexMediaType MediaType { get; init; }

    public required int PlexLibraryId { get; init; }

    public required int Skip { get; init; }

    public required int Take { get; init; }

    public required bool FilterOfflineMedia { get; init; }

    public required bool FilterOwnedMedia { get; init; }

    public required int CountryId { get; init; }

    public required int ActorId { get; init; }

    public required int GenreId { get; init; }
}
