using Reaparr.Domain;

namespace Reaparr.PublicAPI;

public static class TorznabCategoryExtensions
{
    /// <summary>
    /// Determine Torznab category for a movie release.
    /// Torznab only distinguishes SD, HD, and UHD.
    /// </summary>
    public static int ToTorznabMovieCategory(this BasePlexMediaData part)
    {
        var isUhd = IsUhdResolution(part.VideoResolution);

        return part.Source switch
        {
            ReleaseSource.DVD => (int)TorznabCategoryId.Movies_SD,

            _ when isUhd => (int)TorznabCategoryId.Movies_UHD,

            _ => (int)TorznabCategoryId.Movies_HD,
        };
    }

    /// <summary>
    /// Determine Torznab category for a TV episode release.
    /// Torznab only distinguishes SD, HD, and UHD.
    /// </summary>
    public static int ToTorznabEpisodeCategory(this BasePlexMediaData part)
    {
        var isUhd = IsUhdResolution(part.VideoResolution);

        return part.Source switch
        {
            ReleaseSource.DVD => (int)TorznabCategoryId.TV_SD,

            _ when isUhd => (int)TorznabCategoryId.TV_UHD,

            _ => (int)TorznabCategoryId.TV_HD,
        };
    }

    private static bool IsUhdResolution(string resolution)
    {
        if (string.IsNullOrWhiteSpace(resolution))
            return false;

        var lower = resolution.ToLowerInvariant();
        return lower.Contains("2160") || lower.Contains("4k") || lower.Contains("uhd");
    }
}
