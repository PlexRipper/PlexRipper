using System.Diagnostics.CodeAnalysis;

namespace Application.Contracts;

public record RefreshPlexAccountAccessRapportDTO
{
    [SetsRequiredMembers]
    public RefreshPlexAccountAccessRapportDTO(int plexAccountId, string plexAccountName)
    {
        PlexAccountId = plexAccountId;
        PlexAccountName = plexAccountName;
    }

    public required int PlexAccountId { get; init; }

    public required string PlexAccountName { get; set; }

    public required List<PlexServerAccessRapportDTO> Access { get; init; } = new();
}
