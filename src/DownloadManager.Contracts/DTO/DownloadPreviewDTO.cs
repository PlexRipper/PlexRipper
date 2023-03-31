using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public class DownloadPreviewDTO
{
    public int Id { get; set; }

    public string Title { get; set; }

    public long MediaSize { get; set; }

    public int ChildCount { get; set; }
    public PlexMediaType Type { get; set; }

    #region MyRegion

    public int TvShowId { get; set; }

    public int SeasonId { get; set; }

    #endregion

    public List<DownloadPreviewDTO> Children { get; set; } = new();
}