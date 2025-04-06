namespace Application.Contracts;

public class PlexMediaDTO : PlexMediaSlimDTO
{
    public required bool HasArt { get; set; }

    public required bool HasTheme { get; set; }

    public required string Studio { get; set; }

    public required string Summary { get; set; }

    public required string? ContentRating { get; set; }

    public required double Rating { get; set; }

    public required DateTime? OriginallyAvailableAt { get; set; }

    public required int TvShowId { get; set; }

    public required int TvShowSeasonId { get; set; }

    public required List<PlexMediaDataDTO> MediaData { get; set; } = new();

    public required List<PlexMediaDTO> Children { get; set; } = new();
}
