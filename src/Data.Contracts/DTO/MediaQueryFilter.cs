using PlexRipper.Domain;

namespace Data.Contracts;

public record MediaQueryFilter
{
    public required PlexMediaType MediaType { get; set; }

    public required int PlexLibraryId { get; set; }

    public required int Skip { get; set; }

    public required int Take { get; set; }

    public required bool FilterOfflineMedia { get; set; }

    public required bool FilterOwnedMedia { get; set; }

    public required int CountryId { get; set; }

    public required int RoleId { get; set; }

    public required int GenreId { get; set; }
}
