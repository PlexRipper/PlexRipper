using System.Text.RegularExpressions;

namespace Reaparr.PlexApi;

public static partial class MediaFileNameExtensions
{
    [GeneratedRegex(@"(S\d+E\d+|S\d{2,}E\d{2,}|\d+x\d+|\d{4}[-.\s]\d{2}[-.\s]\d{2})", RegexOptions.IgnoreCase)]
    private static partial Regex EpisodeIdentifierRegex();

    [GeneratedRegex(@"\b(19\d{2}|20\d{2}|20[3-9]\d)\b")]
    private static partial Regex YearRegex();

    [GeneratedRegex(@"(?:\{|\[)?(?:tmdb|imdb)[-:](\d+|tt\d+)(?:\}|\])?|tt\d{7,}", RegexOptions.IgnoreCase)]
    private static partial Regex ExplicitIdRegex();

    [GeneratedRegex(
        @"\b(WEBDL|WEB-DL|WEBRip|WEB-Rip|BluRay|BDRip|BRRip|HDRip|HDTV|DVDRip|Remux)\b",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex SourceTagRegex();

    [GeneratedRegex(@"\b(720p|1080p|2160p|4k|uhd|480p|576p|1440p|8k)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ResolutionRegex();

    [GeneratedRegex(@"\b(x264|x265|h264|h265|AVC|HEVC|XviD|DivX|VP9|AV1)\b", RegexOptions.IgnoreCase)]
    private static partial Regex VideoCodecRegex();

    [GeneratedRegex(@"\b(AAC|AC3|EAC3|DTS|DTS-HD|TrueHD|FLAC|Opus|MP3|Atmos)\b", RegexOptions.IgnoreCase)]
    private static partial Regex AudioTokenRegex();

    [GeneratedRegex(@"\b(CD\d+|Part\d+|Disc\d+|DVD\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex MultiPartRegex();

    [GeneratedRegex(@"[a-zA-Z]")]
    private static partial Regex LetterRegex();

    /// <summary>
    /// Validates whether a media filename is already in a format sufficient for Sonarr/Radarr parsing,
    /// indicating it does not require conversion. The filename must contain:
    /// - At least one letter
    /// - Identity information (episode identifiers like S01E02, year, or explicit IDs like tmdb/imdb)
    /// - A source tag (WEBDL, WEBRip, BluRay, HDTV, DVDRip, Remux, etc.)
    /// - Technical information (resolution, video codec, audio codec, or multi-part tokens)
    /// </summary>
    /// <param name="fileName">The filename to validate (can be a full path or just the filename)</param>
    /// <returns>True if the filename is already in a parseable format; otherwise, false</returns>
    public static bool IsValidMediaFileName(this string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var upper = fileName.ToUpperInvariant();

        if (!LetterRegex().IsMatch(fileName))
            return false;

        var hasIdentity =
            EpisodeIdentifierRegex().IsMatch(upper)
            || YearRegex().IsMatch(fileName)
            || ExplicitIdRegex().IsMatch(fileName);

        var hasSource = SourceTagRegex().IsMatch(upper);

        var hasTech =
            ResolutionRegex().IsMatch(upper)
            || VideoCodecRegex().IsMatch(upper)
            || AudioTokenRegex().IsMatch(upper)
            || MultiPartRegex().IsMatch(upper);

        return hasIdentity && hasSource && hasTech;
    }
}
