using System.Text.RegularExpressions;

namespace Reaparr.BackgroundJobs;

public static class MediaSpecMapper
{
    private static readonly ILogger _log = new LogConfig().CreateLogInstance(typeof(MediaSpecMapper));

    public static string MapVideoCodec(this string codec)
    {
        if (string.IsNullOrWhiteSpace(codec))
            return "Unknown";

        switch (codec.ToLowerInvariant())
        {
            case "av1":
                return "AV1";
            case "h264":
                return "x264";
            case "hevc":
                return "x265";
            case "mpeg4":
                return "MPEG-4";
            case "vc1":
                return "VC-1";
            default:
                _log.Here().Warning("Unrecognized video codec: {Codec}", codec);
                return codec.CapitalizeFirst();
        }
    }

    public static string MapAudioCodec(this string codec, string? profile)
    {
        if (string.IsNullOrEmpty(codec))
        {
            return "Unknown";
        }

        var lowerCodec = codec.ToLowerInvariant();
        var lowerProfile = profile?.ToLowerInvariant() ?? string.Empty;

        // Check for DTS-HD MA variants
        if (lowerCodec == "dca" && lowerProfile.Contains("ma", StringComparison.OrdinalIgnoreCase))
        {
            return "DTS-HD.MA";
        }

        if (lowerCodec is "dts-hd ma" or "dts-hd.ma")
        {
            return "DTS-HD.MA";
        }

        if (lowerCodec is "dts-hd" or "dtshd")
        {
            return "DTS-HD";
        }

        switch (lowerCodec)
        {
            case "truehd":
                return "TrueHD";
            case "eac3":
                return "DDP";
            case "ac3":
                return "AC3";
            case "aac":
                return "AAC";
            case "dca":
                return "DCA";
            default:
                _log.Here().Warning("Unrecognized audio codec: {Codec}", codec);
                return codec.CapitalizeFirst();
        }
    }

    public static string MapToFormattedChannels(this int channels)
    {
        switch (channels)
        {
            case 1:
                return "1.0";
            case 2:
                return "2.0";
            case 6:
                return "5.1";
            case 8:
                return "7.1";
            default:
                _log.Here().Warning("Unrecognized audio channel count: {Channels}", channels);
                return $"{channels}.0";
        }
    }

    public static string? ToFileNameSpec(this ReleaseSource source)
    {
        // Note: Remux is handled as a flag (isRemux), not a source enum value
        // When remux is detected, a source is BluRay with isRemux=true
        switch (source)
        {
            case ReleaseSource.BluRay:
                return "BluRay";
            case ReleaseSource.WebDl:
                return "WEB-DL";
            case ReleaseSource.WebRip:
                return "WEBRip";
            case ReleaseSource.DVD:
                return "DVD";
            case ReleaseSource.HDTV:
                return "HDTV";
            case ReleaseSource.Remux:
                return "BluRay"; // Legacy support - should not occur with new logic
            default:
                _log.Here().Warning("Unrecognized release source: {Source}", source);
                return null;
        }
    }

    /// <summary>
    /// Format language (omit English, use MULTI if multiple non-English languages)
    /// </summary>
    /// <param name="streams"></param>
    public static string FormatLanguage(this List<LibraryMediaItemStreamDTO> streams)
    {
        var audioStreams = streams.Where(s => s.StreamType == StreamType.Audio).ToList();

        if (streams.Count == 0)
            return string.Empty;

        // Extract all non-English language codes
        var nonEnglishLanguages = audioStreams
            .Select(a => a.LanguageCode)
            .Where(lang => !IsEnglish(lang))
            .Select(lang =>
            {
                var lowerLang = lang.ToLowerInvariant();
                var match = Regex.Match(lowerLang, @"^([a-z]{2,3})");
                return match.Success ? match.Groups[1].Value.ToUpperInvariant() : string.Empty;
            })
            .Where(lang => !string.IsNullOrEmpty(lang))
            .Distinct()
            .ToList();

        if (nonEnglishLanguages.Count == 0)
            return string.Empty;

        // If multiple non-English languages exist, return MULTI
        if (nonEnglishLanguages.Count > 1)
            return "MULTI";

        // Single non-English language
        return nonEnglishLanguages[0];
    }

    private static bool IsEnglish(string lang)
    {
        if (string.IsNullOrEmpty(lang))
            return false;

        var lowerLang = lang.ToLowerInvariant();
        return lowerLang == "en" || lowerLang == "eng" || lowerLang == "english";
    }
}
