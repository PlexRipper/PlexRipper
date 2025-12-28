namespace Reaparr.PublicAPI;

public static class TorznabCategoryExtensions
{
    /// <summary>
    /// Determine Torznab category for a movie release.
    /// Radarr only differentiates UHD vs non-UHD.
    /// </summary>
    public static int ToTorznabMovieCategory(this BasePlexMediaDataPart part)
    {
        return part.Source >= 2160 ? (int)TorznabCategoryId.Movies_UHD : (int)TorznabCategoryId.Movies;
    }

    /// <summary>
    /// Determine Torznab category for a TV episode release.
    /// Sonarr only differentiates UHD vs non-UHD.
    /// </summary>
    public static int ToTorznabEpisodeCategory(this BasePlexMediaDataPart part) =>
        part.Height >= 2160 ? (int)TorznabCategoryId.TV_UHD : (int)TorznabCategoryId.TV;
}
