using PlexRipper.WebAPI.Common.DTO.PlexMediaData;

namespace PlexRipper.WebAPI.Common.DTO;

public class PlexMediaDTO
{
    public int Id { get; set; }

    public int Key { get; set; }

    /// <summary>
    /// Used specifically for the treeView display in the client
    /// </summary>

    public string TreeKeyId { get; set; }

    public string Title { get; set; }

    public int Year { get; set; }

    public int Duration { get; set; }

    public long MediaSize { get; set; }

    public bool HasThumb { get; set; }

    public bool HasArt { get; set; }

    public bool HasBanner { get; set; }

    public bool HasTheme { get; set; }

    public int Index { get; set; }

    public string Studio { get; set; }

    public string Summary { get; set; }

    public string ContentRating { get; set; }

    public double Rating { get; set; }

    public int ChildCount { get; set; }

    public DateTime AddedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? OriginallyAvailableAt { get; set; }

    public int TvShowId { get; set; }

    public int TvShowSeasonId { get; set; }

    public int PlexLibraryId { get; set; }

    public int PlexServerId { get; set; }

    public PlexMediaType Type { get; set; }

    public List<PlexMediaDataDTO> MediaData { get; set; }

    public List<PlexMediaDTO> Children { get; set; } = new();
}