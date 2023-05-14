namespace PlexRipper.WebAPI.Common.DTO;

public class PlexMediaSlimDTO
{
    public int Id { get; set; }

    public int Index { get; set; }

    public string Title { get; set; }

    public string SortTitle { get; set; }

    public int Year { get; set; }

    public int Duration { get; set; }

    public long MediaSize { get; set; }

    public int ChildCount { get; set; }

    public DateTime AddedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int PlexLibraryId { get; set; }

    public int PlexServerId { get; set; }

    public PlexMediaType Type { get; set; }

    public bool HasThumb { get; set; }

    public List<PlexMediaQuality> Qualities { get; set; }

    public List<PlexMediaSlimDTO> Children { get; set; } = new();
}