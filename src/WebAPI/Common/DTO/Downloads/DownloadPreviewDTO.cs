namespace PlexRipper.WebAPI.Common.DTO;

public class DownloadPreviewDTO
{
    public int Id { get; set; }

    public string Title { get; set; }

    public long Size { get; set; }

    public int ChildCount { get; set; }

    public PlexMediaType MediaType { get; set; }

    public List<DownloadPreviewDTO> Children { get; set; } = new();
}