namespace PlexRipper.Application;

public record RefreshPlexAccountAccessRapportDTO()
{
    public required int PlexAccountId { get; init; }

    public required PlexServerAccessRapport ServerAccessRapport { get; init; }

    public required List<PlexLibraryAccessCrudRapport> LibraryAccessRapport { get; init; }
}
