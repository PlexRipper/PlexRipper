using Reaparr.Logging;

namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, FileSystemEntityType> _fileSystemEntityTypeMap = new(
        StringComparer.Ordinal
    )
    {
        ["Parent"] = FileSystemEntityType.Parent,
        ["Drive"] = FileSystemEntityType.Drive,
        ["Folder"] = FileSystemEntityType.Folder,
        ["File"] = FileSystemEntityType.File,
    };

    /// <summary>
    /// Converts string to <see cref="FileSystemEntityType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="FileSystemEntityType"/>.</param>
    /// <returns>The converted enum of type <see cref="FileSystemEntityType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static FileSystemEntityType ToFileSystemEntityType(this string value)
    {
        if (_fileSystemEntityTypeMap.TryGetValue(value, out var type))
            return type;

        _log.Here()
            .Error(
                "Failed to convert string {Value} to type {NameOfFileSystemEntityType}",
                value,
                nameof(FileSystemEntityType)
            );
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts <see cref="FileSystemEntityType"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="FileSystemEntityType"/>.</param>
    /// <returns>The string value of the <see cref="FileSystemEntityType"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToFileSystemEntityTypeString(this FileSystemEntityType value)
    {
        return value switch
        {
            FileSystemEntityType.Parent => "Parent",
            FileSystemEntityType.Drive => "Drive",
            FileSystemEntityType.Folder => "Folder",
            FileSystemEntityType.File => "File",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here()
                .Error(
                    "Failed to convert {Value} to string of type {NameOfFileSystemEntityType}",
                    value,
                    nameof(FileSystemEntityType)
                );
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
