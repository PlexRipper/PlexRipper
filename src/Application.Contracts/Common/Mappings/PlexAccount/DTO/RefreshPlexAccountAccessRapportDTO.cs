namespace Application.Contracts;

public record RefreshPlexAccountAccessRapportDTO()
{
    public required int PlexAccountId { get; init; }

    public required string PlexAccountName { get; set; }

    public required List<PlexServerAccessRapportDTO> Access { get; init; }
}
