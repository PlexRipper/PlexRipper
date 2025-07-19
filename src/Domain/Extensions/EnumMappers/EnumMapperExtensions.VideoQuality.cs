namespace PlexRipper.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, VideoQuality> _videoQualityMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Standard Definition
            ["sd"] = VideoQuality.SD,
            ["480"] = VideoQuality.SD,
            ["480p"] = VideoQuality.SD,

            // PAL SD
            ["576"] = VideoQuality.DVD,
            ["576p"] = VideoQuality.DVD,

            // HD
            ["720"] = VideoQuality.HD,
            ["720p"] = VideoQuality.HD,

            // Full HD
            ["1080"] = VideoQuality.FullHD,
            ["1080p"] = VideoQuality.FullHD,

            // Quad HD / 2K
            ["1440"] = VideoQuality.QHD,
            ["1440p"] = VideoQuality.QHD,
            ["2k"] = VideoQuality.QHD,

            // 4K / UHD
            ["2160"] = VideoQuality.UHD_4K,
            ["2160p"] = VideoQuality.UHD_4K,
            ["4k"] = VideoQuality.UHD_4K,
            ["uhd"] = VideoQuality.UHD_4K,
            ["uhd1"] = VideoQuality.UHD_4K,

            // 8K
            ["4320"] = VideoQuality.UHD_8K,
            ["4320p"] = VideoQuality.UHD_8K,
            ["8k"] = VideoQuality.UHD_8K,
        };

    public static VideoQuality ToVideoQuality(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return VideoQuality.Unknown;

        return _videoQualityMap.GetValueOrDefault(value.Trim().ToLowerInvariant(), VideoQuality.Unknown);
    }
}
