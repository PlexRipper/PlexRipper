namespace Application.Contracts;

public class PlexMediaDTO : PlexMediaSlimDTO
{
    public int Key { get; set; }

    public bool HasArt { get; set; }

    public bool HasBanner { get; set; }

    public bool HasTheme { get; set; }

    public string Studio { get; set; }

    public string Summary { get; set; }

    public string ContentRating { get; set; }

    public double Rating { get; set; }

    public DateTime? OriginallyAvailableAt { get; set; }

    public int TvShowId { get; set; }

    public int TvShowSeasonId { get; set; }
    public string FullThumbUrl { get; set; } = string.Empty;

    public List<PlexMediaDataDTO> MediaData { get; set; }

    public new List<PlexMediaDTO> Children { get; set; } = new();
}