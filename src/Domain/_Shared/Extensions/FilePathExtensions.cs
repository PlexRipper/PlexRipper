namespace Reaparr.Domain;

public static class FilePathExtensions
{
    private const string TEMP_DOWNLOAD_FILE_SUFFIX = ".reaptemp";

    public static string AddReaparrTempSuffixToFileName(this string fileName) =>
        $"{Path.GetFileNameWithoutExtension(fileName)}{Path.GetExtension(fileName)}{TEMP_DOWNLOAD_FILE_SUFFIX}";

    public static string RemoveReapTempSuffix(this string filePath) =>
        filePath.EndsWith(TEMP_DOWNLOAD_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase)
            ? filePath[..^TEMP_DOWNLOAD_FILE_SUFFIX.Length]
            : filePath;
}
