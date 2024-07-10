using Logging.Interface;

namespace PlexRipper.Domain;

public static class EnumExtensions
{
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(EnumExtensions));

    #region NotificationLevel

    /// <summary>
    /// Converts string to <see cref="NotificationLevel"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="NotificationLevel"/>.</param>
    /// <returns>The converted enum of type <see cref="NotificationLevel"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static NotificationLevel ToNotificationLevel(this string value)
    {
        return value switch
        {
            "None" => NotificationLevel.None,
            "Verbose" => NotificationLevel.Verbose,
            "Debug" => NotificationLevel.Debug,
            "Information" => NotificationLevel.Information,
            "Success" => NotificationLevel.Success,
            "Warning" => NotificationLevel.Warning,
            "Error" => NotificationLevel.Error,
            "Fatal" => NotificationLevel.Fatal,
            _ => DefaultException(),
        };

        NotificationLevel DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfNotificationLevel}", value, nameof(NotificationLevel));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
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
            _log.Here().Error("Failed to convert {Value} to string of type {NameOfNotificationLevel}", value, nameof(NotificationLevel));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    #endregion

    #region ViewMode

    /// <summary>
    /// Converts string to <see cref="ViewMode"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="ViewMode"/>.</param>
    /// <returns>The converted enum of type <see cref="ViewMode"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static ViewMode ToViewMode(this string value)
    {
        return value switch
        {
            "Poster" => ViewMode.Poster,
            "Table" => ViewMode.Table,
            _ => DefaultException(),
        };

        ViewMode DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfNotificationLevel}", value, nameof(ViewMode));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
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

    #endregion

    #region PlexMediaType

    /// <summary>
    /// Converts string to <see cref="PlexMediaType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="PlexMediaType"/>.</param>
    /// <returns>The converted enum of type <see cref="PlexMediaType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static PlexMediaType ToPlexMediaType(this string value)
    {
        return value switch
        {
            "None" => PlexMediaType.None,
            "Movie" => PlexMediaType.Movie,
            "movie" => PlexMediaType.Movie,
            "TvShow" => PlexMediaType.TvShow,
            "show" => PlexMediaType.TvShow,
            "Season" => PlexMediaType.Season,
            "Episode" => PlexMediaType.Episode,
            "Music" => PlexMediaType.Music,
            "Album" => PlexMediaType.Album,
            "Song" => PlexMediaType.Song,
            "Photos" => PlexMediaType.Photos,
            "OtherVideos" => PlexMediaType.OtherVideos,
            "Games" => PlexMediaType.Games,
            "Unknown" => PlexMediaType.Unknown,
            _ => DefaultException(),
        };

        PlexMediaType DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfPlexMediaType}", value, nameof(PlexMediaType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    /// <summary>
    /// Converts <see cref="PlexMediaType"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="PlexMediaType"/>.</param>
    /// <returns>The string value of the <see cref="PlexMediaType"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToPlexMediaTypeString(this PlexMediaType value)
    {
        return value switch
        {
            PlexMediaType.None => "None",
            PlexMediaType.Movie => "Movie",
            PlexMediaType.TvShow => "TvShow",
            PlexMediaType.Season => "Season",
            PlexMediaType.Episode => "Episode",
            PlexMediaType.Music => "Music",
            PlexMediaType.Album => "Album",
            PlexMediaType.Song => "Song",
            PlexMediaType.Photos => "Photos",
            PlexMediaType.OtherVideos => "OtherVideos",
            PlexMediaType.Games => "Games",
            PlexMediaType.Unknown => "Unknown",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here().Error("Failed to convert {Value} to string of type {NameOfPlexMediaType}", value, nameof(PlexMediaType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    #endregion

    #region FolderPath

    /// <summary>
    /// Converts string to <see cref="FolderType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="FolderType"/>.</param>
    /// <returns>The converted enum of type <see cref="FolderType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static FolderType ToFolderType(this string value)
    {
        return value switch
        {
            "None" => FolderType.None,
            "DownloadFolder" => FolderType.DownloadFolder,
            "MovieFolder" => FolderType.MovieFolder,
            "TvShowFolder" => FolderType.TvShowFolder,
            "MusicFolder" => FolderType.MusicFolder,
            "PhotosFolder" => FolderType.PhotosFolder,
            "OtherVideosFolder" => FolderType.OtherVideosFolder,
            "GamesVideosFolder" => FolderType.GamesVideosFolder,
            "Unknown" => FolderType.Unknown,
            _ => DefaultException(),
        };

        FolderType DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfFolderType}", value, nameof(FolderType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
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
            _log.Here().Error("Failed to convert {Value} to string of type {NameOfFolderType}", value, nameof(FolderType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    #endregion

    #region DownloadStatus

    /// <summary>
    /// Converts string to <see cref="DownloadStatus"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="DownloadStatus"/>.</param>
    /// <returns>The converted enum of type <see cref="DownloadStatus"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static DownloadStatus ToDownloadStatus(this string value)
    {
        return value switch
        {
            "Unknown" => DownloadStatus.Unknown,
            "Queued" => DownloadStatus.Queued,
            "Downloading" => DownloadStatus.Downloading,
            "DownloadFinished" => DownloadStatus.DownloadFinished,
            "Completed" => DownloadStatus.Completed,
            "Paused" => DownloadStatus.Paused,
            "Stopped" => DownloadStatus.Stopped,
            "Deleted" => DownloadStatus.Deleted,
            "Merging" => DownloadStatus.Merging,
            "Moving" => DownloadStatus.Moving,
            "Error" => DownloadStatus.Error,
            _ => DefaultException(),
        };

        DownloadStatus DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfDownloadStatus}", value, nameof(DownloadStatus));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    /// <summary>
    /// Converts <see cref="DownloadStatus"/> to string by a fast method.
    /// </summary>
    /// <param name="value">The enum of type <see cref="DownloadStatus"/>.</param>
    /// <returns>The string value of the <see cref="DownloadStatus"/> property.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static string ToDownloadStatusString(this DownloadStatus value)
    {
        return value switch
        {
            DownloadStatus.Unknown => "Unknown",
            DownloadStatus.Queued => "Queued",
            DownloadStatus.Downloading => "Downloading",
            DownloadStatus.DownloadFinished => "DownloadFinished",
            DownloadStatus.Completed => "Completed",
            DownloadStatus.Paused => "Paused",
            DownloadStatus.Stopped => "Stopped",
            DownloadStatus.Deleted => "Deleted",
            DownloadStatus.Merging => "Merging",
            DownloadStatus.Moving => "Moving",
            DownloadStatus.Error => "Error",
            _ => DefaultException(),
        };

        string DefaultException()
        {
            _log.Here().Error("Failed to convert {Value} to string of type {NameOfDownloadStatus}", value, nameof(DownloadStatus));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    #endregion

    #region FileSystemEntityType

    /// <summary>
    /// Converts string to <see cref="FileSystemEntityType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="FileSystemEntityType"/>.</param>
    /// <returns>The converted enum of type <see cref="FileSystemEntityType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static FileSystemEntityType ToFileSystemEntityType(this string value)
    {
        return value switch
        {
            "Parent" => FileSystemEntityType.Parent,
            "Drive" => FileSystemEntityType.Drive,
            "Folder" => FileSystemEntityType.Folder,
            "File" => FileSystemEntityType.File,
            _ => DefaultException(),
        };

        FileSystemEntityType DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfFileSystemEntityType}", value, nameof(FileSystemEntityType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
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
            _log.Here().Error("Failed to convert {Value} to string of type {NameOfFileSystemEntityType}", value, nameof(FileSystemEntityType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    #endregion

    #region DownloadTaskType

    /// <summary>
    /// Converts string to <see cref="DownloadTaskType"/> by a fast method.
    /// </summary>
    /// <param name="value">The string representation of <see cref="DownloadTaskType"/>.</param>
    /// <returns>The converted enum of type <see cref="DownloadTaskType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws exception if value is not found.</exception>
    public static DownloadTaskType ToDownloadTaskType(this string value)
    {
        return value switch
        {
            "None" => DownloadTaskType.None,
            "Movie" => DownloadTaskType.Movie,
            "MovieData" => DownloadTaskType.MovieData,
            "MoviePart" => DownloadTaskType.MoviePart,
            "TvShow" => DownloadTaskType.TvShow,
            "Season" => DownloadTaskType.Season,
            "Episode" => DownloadTaskType.Episode,
            "EpisodeData" => DownloadTaskType.EpisodeData,
            "EpisodePart" => DownloadTaskType.EpisodePart,
            _ => DefaultException(),
        };

        DownloadTaskType DefaultException()
        {
            _log.Here().Error("Failed to convert string {Value} to type {NameOfDownloadTaskType}", value, nameof(DownloadTaskType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
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
            _log.Here().Error("Failed to convert {Value} to string of type {NameOfDownloadTaskType}", value, nameof(DownloadTaskType));
            throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    public static bool IsDownloadable(this DownloadTaskType value) => value switch
    {
        DownloadTaskType.Movie or DownloadTaskType.TvShow or DownloadTaskType.Season or DownloadTaskType.Episode or DownloadTaskType.None => false,
        DownloadTaskType.MovieData or DownloadTaskType.MoviePart or DownloadTaskType.EpisodeData or DownloadTaskType.EpisodePart => true,
        var _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    #endregion
}