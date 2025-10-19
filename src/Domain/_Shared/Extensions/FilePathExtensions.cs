namespace Reaparr.Domain;

public static class FilePathExtensions
{
    private const string TempDownloadFileSuffix = ".reaptemp";

    public static string AddReaparrTempSuffixToFileName(this string fileName) =>
        $"{Path.GetFileNameWithoutExtension(fileName)}{Path.GetExtension(fileName)}{TempDownloadFileSuffix}";

    public static string RemoveReapTempSuffix(this string filePath) =>
        filePath.EndsWith(TempDownloadFileSuffix, StringComparison.OrdinalIgnoreCase)
            ? filePath[..^TempDownloadFileSuffix.Length]
            : filePath;
}
