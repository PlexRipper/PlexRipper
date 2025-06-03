using PlexRipper.Domain;

namespace PlexApi.Contracts;

public record LibraryMetadata
{
    public required PlexLibrary Library { get; set; }

    public List<LibraryMediaItemCountryDTO> Countries { get; set; } = [];

    public List<LibraryMediaItemGenreDTO> Genres { get; set; } = [];

    public List<LibraryMediaItemRoleDTO> Roles { get; set; } = [];
}
