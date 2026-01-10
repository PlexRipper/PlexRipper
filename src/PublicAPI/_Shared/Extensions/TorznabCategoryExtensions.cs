namespace Reaparr.PublicAPI;

public static class TorznabCategoryExtensions
{
    /// <summary>
    /// Determine Torznab category for a movie release.
    /// Torznab only distinguishes SD, HD, and UHD.
    /// </summary>
    public static int ToTorznabMovieCategory(this BasePlexMediaData part)
    {
        var isSd = IsSdResolution(part.VideoResolution);
        var isUhd = IsUhdResolution(part.VideoResolution);

        return part.Source switch
        {
            ReleaseSource.DVD => (int)TorznabCategoryId.Movies_SD,

            _ when isUhd => (int)TorznabCategoryId.Movies_UHD,

            _ when isSd => (int)TorznabCategoryId.Movies_SD,

            _ => (int)TorznabCategoryId.Movies_HD,
        };
    }

    /// <summary>
    /// Determine Torznab category for a TV episode release.
    /// Torznab only distinguishes SD, HD, and UHD.
    /// </summary>
    public static int ToTorznabEpisodeCategory(this BasePlexMediaData part)
    {
        var isSd = IsSdResolution(part.VideoResolution);
        var isUhd = IsUhdResolution(part.VideoResolution);

        return part.Source switch
        {
            ReleaseSource.DVD => (int)TorznabCategoryId.TV_SD,

            _ when isUhd => (int)TorznabCategoryId.TV_UHD,

            _ when isSd => (int)TorznabCategoryId.TV_SD,

            _ => (int)TorznabCategoryId.TV_HD,
        };
    }

    private static bool IsUhdResolution(VideoQuality resolution) =>
        resolution switch
        {
            VideoQuality.UHD_4K => true,
            VideoQuality.UHD_8K => true,
            _ => false,
        };

    private static bool IsSdResolution(VideoQuality resolution) =>
        resolution switch
        {
            VideoQuality.SD => true,
            VideoQuality.DVD => true,
            _ => false,
        };
}
