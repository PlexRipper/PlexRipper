namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, ViewMode> _viewModeMap = new(StringComparer.Ordinal)
    {
        ["Poster"] = ViewMode.Poster,
        ["Table"] = ViewMode.Table,
    };

    /// <summary>
    /// Converts string to <see cref="ViewMode"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="ViewMode"/>.</param>
    /// <returns>The converted enum of type <see cref="ViewMode"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static ViewMode ToViewMode(this string value)
    {
        if (_viewModeMap.TryGetValue(value, out var mode))
            return mode;

        _log.Here().Error("Failed to convert string {Value} to type {NameOfViewMode}", value, nameof(ViewMode));
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts <see cref="ViewMode"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="ViewMode"/>.</param>
    /// <returns>The string value of the <see cref="ViewMode"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToViewModeString(this ViewMode value)
    {
        return value switch
        {
            ViewMode.Poster => "Poster",
            ViewMode.Table => "Table",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfViewMode)}", value, nameof(ViewMode));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
