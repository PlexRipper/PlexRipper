using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public class DownloadPreview
{
    public int Id { get; set; }

    public string Title { get; set; }

    public long Size { get; set; }

    public int ChildCount { get; set; }
    public PlexMediaType Type { get; set; } = PlexMediaType.Unknown;

    public int TvShowId { get; set; }

    public int SeasonId { get; set; }

    public List<DownloadPreview> Children { get; set; } = new();
}