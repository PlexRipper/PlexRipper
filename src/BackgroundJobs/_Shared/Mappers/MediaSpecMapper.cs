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
            case "mpeg2video":
            case "mpeg2":
                return "MPEG-2";
            case "mpeg1video":
            case "mpeg1":
                return "MPEG-1";
            case "vc1":
                return "VC-1";
            case "vp8":
                return "VP8";
            case "vp9":
                return "VP9";
            case "wmv1":
                return "WMV1";
            case "wmv2":
                return "WMV2";
            case "wmv3":
                return "WMV3";
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

        if (lowerCodec is "dts-hd ma" or "dts-hd.ma" or "dca-ma")
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
            case "dts":
                return "DTS";
            case "mp3":
                return "MP3";
            case "flac":
                return "FLAC";
            case "opus":
                return "Opus";
            case "wmapro":
                return "WMA Pro";
            case "wmalossless":
                return "WMA Lossless";
            case "wmav1":
                return "WMA";
            case "wmav2":
                return "WMA";
            case "pcm":
                return "PCM";
            default:
                _log.Here().Warning("Unrecognized audio codec: {Codec}", codec);
                return codec.CapitalizeFirst();
        }
    }

    public static string MapToFormattedChannels(this int channels)
    {
        switch (channels)
        {
            case 0:
                return "";
            case 1:
                return "1.0";
            case 2:
                return "2.0";
            case 3:
                return "2.1";
            case 4:
                return "4.0";
            case 5:
                return "5.0";
            case 6:
                return "5.1";
            case 7:
                return "6.1";
            case 8:
                return "7.1";
            case 9:
                return "9.1";
            case 10:
                return "9.1";
            case 11:
                return "11.1";
            case 12:
                return "11.1";
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
