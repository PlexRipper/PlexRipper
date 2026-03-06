namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, PlexDownloadClientType> _plexDownloadClientTypes = new(
        StringComparer.Ordinal
    )
    {
        ["Direct"] = PlexDownloadClientType.Direct,
        ["Dash"] = PlexDownloadClientType.Dash,
    };

    /// <summary>
    /// Converts string to <see cref="PlexDownloadClientType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="PlexDownloadClientType"/>.</param>
    /// <returns>The converted enum of type <see cref="PlexDownloadClientType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static PlexDownloadClientType ToPlexDownloadClientType(this string value)
    {
        if (_plexDownloadClientTypes.TryGetValue(value, out var mode))
            return mode;

        _log.Here()
            .Error(
                "Failed to convert string {Value} to type {NameOfPlexDownloadClientType}",
                value,
                nameof(PlexDownloadClientType)
            );
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts <see cref="PlexDownloadClientType"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="PlexDownloadClientType"/>.</param>
    /// <returns>The string value of the <see cref="PlexDownloadClientType"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToPlexDownloadClientTypeString(this PlexDownloadClientType value)
    {
        return value switch
        {
            PlexDownloadClientType.Direct => "Direct",
            PlexDownloadClientType.Dash => "Dash",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here()
                .Error(
                    "Failed to convert enum {Value} of type {NameOfPlexDownloadClientType} to string",
                    value,
                    nameof(PlexDownloadClientType)
                );
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
