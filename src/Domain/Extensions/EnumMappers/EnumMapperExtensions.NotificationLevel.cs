namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, NotificationLevel> _map = new(StringComparer.Ordinal)
    {
        ["None"] = NotificationLevel.None,
        ["Verbose"] = NotificationLevel.Verbose,
        ["Debug"] = NotificationLevel.Debug,
        ["Information"] = NotificationLevel.Information,
        ["Success"] = NotificationLevel.Success,
        ["Warning"] = NotificationLevel.Warning,
        ["Error"] = NotificationLevel.Error,
        ["Fatal"] = NotificationLevel.Fatal,
    };

    public static NotificationLevel ToNotificationLevel(this string value)
    {
        if (_map.TryGetValue(value, out var level))
            return level;

        _log.Here()
            .Error(
                "Failed to convert string {Value} to type {NameOfNotificationLevel}",
                value,
                nameof(NotificationLevel)
            );
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts <see cref="NotificationLevel"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="NotificationLevel"/>.</param>
    /// <returns>The string value of the <see cref="NotificationLevel"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToNotificationLevelString(this NotificationLevel value)
    {
        return value switch
        {
            NotificationLevel.None => "None",
            NotificationLevel.Verbose => "Verbose",
            NotificationLevel.Debug => "Debug",
            NotificationLevel.Information => "Information",
            NotificationLevel.Success => "Success",
            NotificationLevel.Warning => "Warning",
            NotificationLevel.Error => "Error",
            NotificationLevel.Fatal => "Fatal",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here()
                .Error(
                    "Failed to convert {Value} to string of type {NameOfNotificationLevel}",
                    value,
                    nameof(NotificationLevel)
                );
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    public static NotificationLevel ToNotificationLevel(this DownloadStatus value) =>
        value switch
        {
            DownloadStatus.Error or DownloadStatus.ServerUnreachable or DownloadStatus.Unknown =>
                NotificationLevel.Error,
            var _ => NotificationLevel.Information,
        };
}
