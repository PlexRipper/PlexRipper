namespace Application.Contracts;

public record PlexLibraryAccessRefreshResponse
{
    public required List<int> OfflineServers { get; set; } = new();

    public required List<PlexLibraryAccessCrudRapport> Reports { get; set; } = new();
}
