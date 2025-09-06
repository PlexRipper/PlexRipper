namespace Reaparr.Application.Contracts;

public record PlexMediaMetadataDTO
{
    public required int RoleCount { get; init; }

    public required int CountryCount { get; init; }

    public required int GenreCount { get; init; }
    public required int QualityCount { get; init; }
    public required List<PlexRoleDTO> Roles { get; init; }

    public required List<PlexCountryDTO> Countries { get; init; }

    public required List<PlexGenreDTO> Genres { get; init; }

    public required List<PlexQualityDTO> Qualities { get; init; }
}
