using PlexRipper.Domain;

namespace PlexApi.Contracts;

public record LibraryMetadata
{
    public required PlexLibrary Library { get; init; }

    public required IReadOnlyCollection<LibraryMediaItemCountryDTO> Countries { get; init; } = [];

    public required IReadOnlyCollection<LibraryMediaItemGenreDTO> Genres { get; init; } = [];

    public required IReadOnlyCollection<LibraryMediaItemRoleDTO> Actors { get; init; } = [];
}
