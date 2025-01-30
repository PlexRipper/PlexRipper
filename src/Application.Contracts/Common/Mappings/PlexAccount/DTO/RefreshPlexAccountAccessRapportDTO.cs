using PlexRipper.Domain;

namespace Application.Contracts;

public record RefreshPlexAccountAccessRapportDTO()
{
    public required int PlexAccountId { get; init; }

    public required List<PlexServerAccessRapportDTO> Access { get; init; }
}

public record PlexServerAccessRapportDTO()
{
    public int PlexServerId { get; set; }

    public PlexAccessState State { get; set; }

    public List<PlexLibraryAccessRapportDTO> LibraryAccess { get; set; }
}

public record PlexLibraryAccessRapportDTO()
{
    public int PlexServerId { get; set; }

    public int PlexLibraryId { get; set; }

    public PlexAccessState State { get; set; }
}
