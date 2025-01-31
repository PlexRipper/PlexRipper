namespace Application.Contracts;

public record PlexLibraryAccessCrudRapportDTO
{
    public required int PlexServerId { get; set; }

    public required List<int> Created { get; init; } = [];

    public required List<int> Updated { get; init; } = [];

    public required List<int> Deleted { get; init; } = [];
}
