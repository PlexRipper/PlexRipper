using PlexRipper.Application;

namespace Application.Contracts;

public record RefreshPlexAccountAccessRapportDTO()
{
    public required int PlexAccountId { get; init; }

    public required PlexServerAccessRapportDTO ServerAccessRapport { get; init; }

    public required List<PlexLibraryAccessCrudRapportDTO> LibraryAccessRapport { get; init; }
}
