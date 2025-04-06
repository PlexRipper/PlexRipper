using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexServerAccessRapportDTO
{
    public required int PlexServerId { get; set; }

    public required string PlexServerName { get; set; }

    public required PlexAccessState State { get; set; }

    public required bool IsServerOffline { get; set; }

    public required List<PlexLibraryAccessRapportDTO> LibraryAccess { get; set; }
}
