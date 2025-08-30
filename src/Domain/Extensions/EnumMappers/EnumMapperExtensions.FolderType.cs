using Reaparr.Logging;

namespace Reaparr.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, FolderType> _folderTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["None"] = FolderType.None,
        ["DownloadFolder"] = FolderType.DownloadFolder,
        ["MovieFolder"] = FolderType.MovieFolder,
        ["TvShowFolder"] = FolderType.TvShowFolder,
        ["MusicFolder"] = FolderType.MusicFolder,
        ["PhotosFolder"] = FolderType.PhotosFolder,
        ["OtherVideosFolder"] = FolderType.OtherVideosFolder,
        ["GamesVideosFolder"] = FolderType.GamesVideosFolder,
        ["Unknown"] = FolderType.Unknown,
    };

    /// <summary>
    /// Converts string to <see cref="FolderType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="FolderType"/>.</param>
    /// <returns>The converted enum of type <see cref="FolderType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static FolderType ToFolderType(this string value)
    {
        if (_folderTypeMap.TryGetValue(value, out var type))
            return type;

        _log.Here().Error("Failed to convert string {Value} to type {NameOfFolderType}", value, nameof(FolderType));
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts <see cref="FolderType"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="FolderType"/>.</param>
    /// <returns>The string value of the <see cref="FolderType"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToFolderTypeString(this FolderType value)
    {
        return value switch
        {
            FolderType.None => "None",
            FolderType.DownloadFolder => "DownloadFolder",
            FolderType.MovieFolder => "MovieFolder",
            FolderType.TvShowFolder => "TvShowFolder",
            FolderType.MusicFolder => "MusicFolder",
            FolderType.PhotosFolder => "PhotosFolder",
            FolderType.OtherVideosFolder => "OtherVideosFolder",
            FolderType.GamesVideosFolder => "GamesVideosFolder",
            FolderType.Unknown => "Unknown",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here()
                .Error("Failed to convert {Value} to string of type {NameOfFolderType}", value, nameof(FolderType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
