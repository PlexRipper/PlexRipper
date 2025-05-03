namespace Application.Contracts;

public record PlexMediaMetadataDTO
{
    public required List<PlexRoleDTO> Roles { get; init; }

    public required List<PlexCountryDTO> Countries { get; init; }

    public required List<PlexGenreDTO> Genres { get; init; }
}
