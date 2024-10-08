using PlexRipper.Domain;

namespace Application.Contracts;

public class DownloadPreview
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public long Size { get; set; }

    public int ChildCount { get; set; }

    public PlexMediaType MediaType { get; init; } = PlexMediaType.Unknown;

    public int TvShowId { get; init; }

    public int SeasonId { get; init; }

    public List<DownloadPreview> Children { get; init; } = [];
}
