using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Environment;

namespace PlexRipper.Domain;

public static partial class StringExtensions
{
    private static Random random = new();

    private static readonly HashSet<string> StopWords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "a",
            "an",
            "and",
            "the",
            "of",
            "in",
            "on",
            "for",
            "with",
            "to",
            "by",
            "at",
        };

    private static readonly string[] Articles = ["a", "an", "the"];

    public static string GetActualCasing(this string path)
    {
        if (OsInfo.IsNotWindows || path.StartsWith("\\"))
            return path;

        if (Directory.Exists(path) && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
            return GetProperCapitalization(new DirectoryInfo(path));

        var fileInfo = new FileInfo(path);
        var dirInfo = fileInfo.Directory;

        var fileName = fileInfo.Name;

        if (dirInfo != null && fileInfo.Exists)
            fileName = dirInfo.GetFiles(fileInfo.Name)[0].Name;

        return Path.Combine(GetProperCapitalization(dirInfo), fileName);
    }

    public static string RandomString(int length, bool allowNumbers = false, bool allowCapitalLetters = false)
    {
        var chars = "abcdefghijklmnopqrstuvwxyz";

        if (allowCapitalLetters)
            chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        if (allowNumbers)
            chars += "0123456789";

        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static bool IsIpAddress(this string ipAddress) => IPAddress.TryParse(ipAddress, out var _);

    private static string GetProperCapitalization(DirectoryInfo dirInfo)
    {
        var parentDirInfo = dirInfo.Parent;
        if (parentDirInfo == null)
        {
            // Drive letter
            return dirInfo.Name.ToUpper();
        }

        var folderName = dirInfo.Name;

        if (dirInfo.Exists)
            folderName = parentDirInfo.GetDirectories(dirInfo.Name)[0].Name;

        return Path.Combine(GetProperCapitalization(parentDirInfo), folderName);
    }

    /// <summary>
    /// Replaces invalid characters from a file or folder name
    /// Source: https://stackoverflow.com/a/13617375/8205497
    /// </summary>
    /// <param name="name"> The filename or folder name to sanitize. </param>
    /// <returns> The sanitized filename or folder name. </returns>
    public static string SanitizeFolderName(this string name)
    {
        var invalids = Path.GetInvalidFileNameChars();
        name = name.Replace(@"·", "-").Replace(": ", " ");
        return string.Join(" ", name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    public static string AddPartIndexToFileName(this string fileName, int partIndex) =>
        $"{Path.GetFileNameWithoutExtension(fileName)}.part{partIndex}{Path.GetExtension(fileName)}";

    /// <summary>
    /// Converts a title to a sort title that can be used for sorting.
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public static string ToSortTitle(this string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        // Convert to lowercase
        var lowerTitle = title.ToLowerInvariant();

        // Remove leading articles
        var sortTitle = RemoveLeadingArticle(lowerTitle);

        // Remove punctuation and diacritics
        sortTitle = RemoveDiacritics(sortTitle);
        sortTitle = RemoveWhiteSpaceRegex().Replace(sortTitle, "");

        // Trim any leading or trailing whitespace
        return sortTitle.Trim();
    }

    public static string ToSearchTitle(this string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        // Convert to lower case
        var lowerTitle = title.ToLowerInvariant();

        // Remove punctuation and diacritics
        var normalizedTitle = RemoveDiacritics(lowerTitle);
        normalizedTitle = RemoveWhiteSpaceRegex().Replace(normalizedTitle, "");

        // Split title into words and remove stop words
        var words = normalizedTitle
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => !StopWords.Contains(word));

        // Join the words back into a string
        var searchTitle = string.Join(" ", words);

        return searchTitle;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string RemoveLeadingArticle(string title)
    {
        foreach (var article in Articles)
        {
            var articleWithSpace = article + " ";
            if (title.StartsWith(articleWithSpace, StringComparison.OrdinalIgnoreCase))
                return title.Substring(articleWithSpace.Length);
        }

        return title;
    }

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex RemoveWhiteSpaceRegex();

    public static string GetFileName(this string path)
    {
        path = path.Replace(@"\", "/");
        return Path.GetFileName(path);
    }
}
