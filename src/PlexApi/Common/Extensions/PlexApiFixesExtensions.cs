using System.Text.RegularExpressions;

namespace PlexRipper.PlexApi;

public static class PlexApiFixesExtensions
{
    private static readonly Regex[] CompiledPatterns = new[]
    {
        new Regex(@"(?<!\d)(4320p|8K)(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)(2160p|4K)(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)1440p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)1080p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)720p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)576p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)480p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)360p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)240p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(?<!\d)144p(?!\d)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    };

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
        foreach (var regex in CompiledPatterns)
        {
            var match = regex.Match(filename);
            if (match.Success)
                return match.Value.ToLowerInvariant();
        }
        return string.Empty;
    }
}
