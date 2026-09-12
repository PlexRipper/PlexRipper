namespace Reaparr.PublicAPI;

public static class TorznabCategoryExtensions
{
    /// <summary>
    /// Determine Torznab category for a movie release.
    /// Torznab only distinguishes SD, HD, and UHD.
    /// </summary>
    public static int ToTorznabMovieCategory(this BasePlexMediaData part) =>
        ToMovieCategory(part.VideoResolution, part.Source);

    public static int ToTorznabMovieCategory(this TorznabFeedItemProjection item) =>
        ToMovieCategory(item.VideoResolution, item.Source);

    /// <summary>
    /// Determine Torznab category for a TV episode release.
    /// Torznab only distinguishes SD, HD, and UHD.
    /// </summary>
    public static int ToTorznabEpisodeCategory(this BasePlexMediaData part) =>
        ToEpisodeCategory(part.VideoResolution, part.Source);

    public static int ToTorznabEpisodeCategory(this TorznabFeedItemProjection item) =>
        ToEpisodeCategory(item.VideoResolution, item.Source);

    private static int ToMovieCategory(VideoQuality resolution, ReleaseSource source)
    {
        var isSd = IsSdResolution(resolution);
        var isUhd = IsUhdResolution(resolution);

        return source switch
        {
            ReleaseSource.DVD => (int)TorznabCategoryId.Movies_SD,
            _ when isUhd => (int)TorznabCategoryId.Movies_UHD,
            _ when isSd => (int)TorznabCategoryId.Movies_SD,
            _ => (int)TorznabCategoryId.Movies_HD,
        };
    }

    private static int ToEpisodeCategory(VideoQuality resolution, ReleaseSource source)
    {
        var isSd = IsSdResolution(resolution);
        var isUhd = IsUhdResolution(resolution);

        return source switch
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
