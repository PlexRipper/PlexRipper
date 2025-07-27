using System.Text.RegularExpressions;

namespace PlexRipper.PlexApi;

public static class PlexApiFixesExtensions
{
    /// <summary>
    /// Sometimes Plex does not return the quality of a media item.
    /// This extension method will parse the filename and return the quality if it can be determined.
    /// </summary>
    /// <param name="filename"> The filename of the media item to parse.</param>
    /// <returns>The quality of the media item as a string, or an empty string if it cannot be determined.</returns>
    public static string ParseQualityFromFileName(this string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return string.Empty;

        var patterns = new[]
        {
            @"(?<!\d)(4320p|8K)(?!\d)",
            @"(?<!\d)(2160p|4K)(?!\d)",
            @"(?<!\d)1440p(?!\d)",
            @"(?<!\d)1080p(?!\d)",
            @"(?<!\d)720p(?!\d)",
            @"(?<!\d)576p(?!\d)",
            @"(?<!\d)480p(?!\d)",
            @"(?<!\d)360p(?!\d)",
            @"(?<!\d)240p(?!\d)",
            @"(?<!\d)144p(?!\d)",
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(filename, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Value.ToLowerInvariant();
        }

        return string.Empty;
    }
}
