using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexLibraryCountries
{
    public PlexLibraryCountries() { }

    [SetsRequiredMembers]
    public PlexLibraryCountries(int plexLibraryId, int plexCountryId)
    {
        PlexLibraryId = plexLibraryId;
        PlexCountryId = plexCountryId;
    }

    [Column(Order = 1)]
    public required int PlexLibraryId { get; init; }

    [Column(Order = 2)]
    public required int PlexCountryId { get; init; }
}
