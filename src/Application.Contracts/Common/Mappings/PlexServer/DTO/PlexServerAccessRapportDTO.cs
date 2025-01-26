namespace PlexRipper.Application;

public record PlexServerAccessRapportDTO
{
    public required List<int> Created { get; init; } = [];

    public required List<int> Updated { get; init; } = [];

    public required List<int> Deleted { get; init; } = [];
}
