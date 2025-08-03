namespace PlexRipper.Domain;

public static partial class EnumMapperExtensions
{
    private static readonly Dictionary<string, DownloadTaskType> _downloadTaskTypeMap = new(StringComparer.Ordinal)
    {
        ["None"] = DownloadTaskType.None,
        ["Movie"] = DownloadTaskType.Movie,
        ["MovieData"] = DownloadTaskType.MovieData,
        ["MoviePart"] = DownloadTaskType.MoviePart,
        ["TvShow"] = DownloadTaskType.TvShow,
        ["Season"] = DownloadTaskType.Season,
        ["Episode"] = DownloadTaskType.Episode,
        ["EpisodeData"] = DownloadTaskType.EpisodeData,
        ["EpisodePart"] = DownloadTaskType.EpisodePart,
    };

    /// <summary>
    /// Converts string to <see cref="DownloadTaskType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="DownloadTaskType"/>.</param>
    /// <returns>The converted enum of type <see cref="DownloadTaskType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static DownloadTaskType ToDownloadTaskType(this string value)
    {
        if (_downloadTaskTypeMap.TryGetValue(value, out var type))
            return type;

        _log.Here()
            .Error(
                "Failed to convert string {Value} to type {NameOfDownloadTaskType}",
                value,
                nameof(DownloadTaskType)
            );
        throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    /// <summary>
    /// Converts <see cref="DownloadTaskType"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="DownloadTaskType"/>.</param>
    /// <returns>The string value of the <see cref="DownloadTaskType"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToDownloadTaskString(this DownloadTaskType value)
    {
        return value switch
        {
            DownloadTaskType.None => "None",
            DownloadTaskType.Movie => "Movie",
            DownloadTaskType.MovieData => "MovieData",
            DownloadTaskType.MoviePart => "MoviePart",
            DownloadTaskType.TvShow => "TvShow",
            DownloadTaskType.Season => "Season",
            DownloadTaskType.Episode => "Episode",
            DownloadTaskType.EpisodeData => "EpisodeData",
            DownloadTaskType.EpisodePart => "EpisodePart",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here()
                .Error(
                    "Failed to convert {Value} to string of type {NameOfDownloadTaskType}",
                    value,
                    nameof(DownloadTaskType)
                );
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    public static bool IsDownloadable(this DownloadTaskType value) =>
        value switch
        {
            DownloadTaskType.Movie
            or DownloadTaskType.TvShow
            or DownloadTaskType.Season
            or DownloadTaskType.Episode
            or DownloadTaskType.None => false,
            DownloadTaskType.MovieData
            or DownloadTaskType.MoviePart
            or DownloadTaskType.EpisodeData
            or DownloadTaskType.EpisodePart => true,
            var _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

    public static PlexMediaType ToPlexMediaType(this DownloadTaskType value)
    {
        return value switch
        {
            DownloadTaskType.Movie or DownloadTaskType.MovieData or DownloadTaskType.MoviePart => PlexMediaType.Movie,
            DownloadTaskType.TvShow => PlexMediaType.TvShow,
            DownloadTaskType.Season => PlexMediaType.Season,
            DownloadTaskType.Episode or DownloadTaskType.EpisodeData or DownloadTaskType.EpisodePart =>
                PlexMediaType.Episode,
            DownloadTaskType.None => PlexMediaType.None,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}
