namespace Application.Contracts;

public record ServerDownloadProgressDTO
{
    public required int Id { get; set; }

    public required int DownloadableTasksCount { get; set; }

    public required List<DownloadProgressDTO> Downloads { get; init; } = [];
}
