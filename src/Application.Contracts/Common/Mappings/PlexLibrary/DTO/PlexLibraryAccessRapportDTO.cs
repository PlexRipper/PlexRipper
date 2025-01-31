using PlexRipper.Domain;

namespace Application.Contracts;

public record PlexLibraryAccessRapportDTO()
{
    public required int PlexServerId { get; set; }

    public required int PlexLibraryId { get; set; }

    public required PlexAccessState State { get; set; }
}
