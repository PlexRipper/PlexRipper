using Reaparr.Domain;

namespace Reaparr.PublicAPI;

public static class TorznabCategoryExtensions
{
    /// <summary>
    /// Determine Torznab category for a movie release.
    /// Maps ReleaseSource to appropriate Torznab category based on source type and resolution.
    /// </summary>
    public static int ToTorznabMovieCategory(this BasePlexMediaData part)
    {
        var isUhd = IsUhdResolution(part.VideoResolution);

        return part.Source switch
        {
            ReleaseSource.BluRay => (int)TorznabCategoryId.Movies_BluRay,
            ReleaseSource.DVD => (int)TorznabCategoryId.Movies_SD,
            ReleaseSource.Remux or ReleaseSource.WebDl or ReleaseSource.WebRip or ReleaseSource.HDTV => isUhd
                ? (int)TorznabCategoryId.Movies_UHD
                : (int)TorznabCategoryId.Movies_HD,
            ReleaseSource.None => isUhd ? (int)TorznabCategoryId.Movies_UHD : (int)TorznabCategoryId.Movies,
            _ => (int)TorznabCategoryId.Movies,
        };
    }

    /// <summary>
    /// Determine Torznab category for a TV episode release.
    /// Maps ReleaseSource to appropriate Torznab category based on source type and resolution.
    /// </summary>
    public static int ToTorznabEpisodeCategory(this BasePlexMediaData part)
    {
        var isUhd = IsUhdResolution(part.VideoResolution);

        return part.Source switch
        {
            ReleaseSource.DVD => (int)TorznabCategoryId.TV_SD,
            ReleaseSource.Remux
            or ReleaseSource.BluRay
            or ReleaseSource.WebDl
            or ReleaseSource.WebRip
            or ReleaseSource.HDTV => isUhd ? (int)TorznabCategoryId.TV_UHD : (int)TorznabCategoryId.TV_HD,
            ReleaseSource.None => isUhd ? (int)TorznabCategoryId.TV_UHD : (int)TorznabCategoryId.TV,
            _ => (int)TorznabCategoryId.TV,
        };
    }

    private static bool IsUhdResolution(string resolution)
    {
        if (string.IsNullOrWhiteSpace(resolution))
        {
            return false;
        }

        var lower = resolution.ToLowerInvariant();
        return lower.Contains("2160") || lower.Contains("4k") || lower.Contains("uhd");
    }
}
