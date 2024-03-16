namespace PlexRipper.WebAPI;

public record ServerDownloadProgressDTO
{
    public int Id { get; set; }

    public int DownloadableTasksCount { get; set; }

    public List<DownloadProgressDTO> Downloads { get; set; }
}