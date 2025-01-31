namespace Application.Contracts;

public record RefreshPlexAccountAccessRapportDTO()
{
    public required int PlexAccountId { get; init; }

    public required List<PlexServerAccessRapportDTO> Access { get; init; }
}
