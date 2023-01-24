namespace Logging;

public static class LogExtensions
{
    #region Base

    private static string FormatForException(this string message, Exception ex)
    {
        return $"{message}: {ex?.ToString() ?? string.Empty}";
    }

    #endregion
}