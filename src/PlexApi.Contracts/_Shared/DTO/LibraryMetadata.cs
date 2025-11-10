using System.Diagnostics.CodeAnalysis;

namespace Reaparr.PlexApi.Contracts;

public record LibraryMetadata
{
    [SetsRequiredMembers]
    public LibraryMetadata(PlexLibrary plexLibrary)
    {
        Library = plexLibrary;
    }

    public PlexLibrary Library { get; private set; }

    public required IReadOnlyCollection<LibraryMediaItemCountryDTO> Countries { get; init; } = [];

    public required IReadOnlyCollection<LibraryMediaItemGenreDTO> Genres { get; init; } = [];

    public required IReadOnlyCollection<LibraryMediaItemRoleDTO> Actors { get; init; } = [];
}
