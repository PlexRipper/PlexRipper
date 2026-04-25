using Xdg.Directories;

namespace Reaparr.Environment;

public class PathProvider : IPathProvider
{
    #region Properties

    #region DirectoryNames

    public static readonly string DefaultConfigFolderName = "Config";

    private static readonly string _logsFolder = "Logs";

    /// <summary>
    /// Gets the sub-folder in some cases such as in Desktop mode.
    /// </summary>
    public static string DefaultReaparrFolderName => "Reaparr";

    /// <summary>
    /// Gets the default folder name used for movie libraries under the root media directory.
    /// </summary>
    public static string DefaultMovieFolderName => "Movies";

    /// <summary>
    /// Gets the default folder name used for downloaded files under the root media directory.
    /// </summary>
    public static string DefaultDownloadsFolderName => "Downloads";

    /// <summary>
    /// Gets the default folder name used for TV show libraries under the root media directory.
    /// </summary>
    public static string DefaultTvShowsFolderName => "TvShows";

    /// <summary>
    /// Gets the default folder name used for music libraries under the root media directory.
    /// </summary>
    public static string DefaultMusicFolderName => "Music";

    /// <summary>
    /// Gets the default folder name used for photo libraries under the root media directory.
    /// </summary>
    public static string DefaultPhotosFolderName => "Photos";

    /// <summary>
    /// Gets the default folder name used for uncategorized media under the root media directory.
    /// </summary>
    public static string DefaultOtherFolderName => "Other";

    /// <summary>
    /// Gets the default folder name used for game libraries under the root media directory.
    /// </summary>
    public static string DefaultGamesFolderName => "Games";

    #region FileNames

    /// <summary>
    /// Gets the file name used for the main Reaparr settings file.
    /// </summary>
    public static string ConfigFileName => "ReaparrSettings.json";

    /// <summary>
    /// Gets the file name used for the SQLite database.
    /// </summary>
    public static string DatabaseName => "ReaparrDB.db";

    /// <summary>
    /// Gets the file name used for the SQLite shared-memory sidecar file.
    /// </summary>
    public static string DatabaseShmName => $"{DatabaseName}-shm";

    /// <summary>
    /// Gets the file name used for the SQLite write-ahead log sidecar file.
    /// </summary>
    public static string DatabaseWalName => $"{DatabaseName}-wal";

    #endregion

    /// <summary>
    /// Gets the default downloads destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public string DefaultDownloadsDestinationFolder
    {
        get
        {
            var downloadsPath = EnvironmentExtensions.GetDownloadsPath();
            if (downloadsPath is not null)
                return downloadsPath;

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultDownloadsFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(DataDirectory, DefaultDownloadsFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    /// <summary>
    /// Gets the default movies destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public string DefaultMovieDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetMoviesPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultMovieFolderName);

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultMovieFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(DataDirectory, DefaultMovieFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    /// <summary>
    /// Gets the default TV shows destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public string DefaultTvShowsDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetTvShowsPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultTvShowsFolderName);

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultTvShowsFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(DataDirectory, DefaultTvShowsFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    /// <summary>
    /// Gets the default music destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public string DefaultMusicDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetMusicPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultMusicFolderName);

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultMusicFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(DataDirectory, DefaultMusicFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    /// <summary>
    /// Gets the default photos destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public string DefaultPhotosDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetPhotosPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultPhotosFolderName);

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultPhotosFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(DataDirectory, DefaultPhotosFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    /// <summary>
    /// Gets the default uncategorized media destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public string DefaultOtherDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetOtherPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultOtherFolderName);

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultOtherFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(DataDirectory, DefaultOtherFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    /// <summary>
    /// Gets the default games destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public string DefaultGamesDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetGamesPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultGamesFolderName);

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultGamesFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(DataDirectory, DefaultGamesFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    #endregion


    /// <summary>
    /// Gets the directory that stores Reaparr's own application state, such as settings, the SQLite database, backups, and logs.
    /// This is distinct from <see cref="DataDirectory"/>, which is the root location for user media content like Movies, TV shows, and Downloads.
    /// </summary>
    public static string ConfigDirectory
    {
        get
        {
            var configPath = EnvironmentExtensions.GetConfigPath();
            if (configPath != null)
                return configPath;

            if (EnvironmentExtensions.IsDockerMode())
                return Path.Combine("/", DefaultConfigFolderName);

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(BaseDirectory.ConfigHome, DefaultReaparrFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    /// <summary>
    /// Gets the full path to the main Reaparr settings file.
    /// </summary>
    public static string ConfigFileLocation => Path.Combine(ConfigDirectory, ConfigFileName);

    /// <summary>
    /// Gets the directory used to store database backup files.
    /// </summary>
    public static string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

    /// <summary>
    /// Gets the full path to the SQLite database file.
    /// </summary>
    public static string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

    /// <summary>
    /// Gets the full path to the SQLite shared-memory sidecar file.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static string Database_SHM_Path => Path.Combine(ConfigDirectory, DatabaseShmName);

    /// <summary>
    /// Gets the full path to the SQLite write-ahead log sidecar file.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static string Database_WAL_Path => Path.Combine(ConfigDirectory, DatabaseWalName);

    /// <summary>
    /// Gets the directory used to store application log files.
    /// </summary>
    public static string LogsDirectory => Path.Combine(ConfigDirectory, _logsFolder);

    /// <summary>
    /// Gets the full set of SQLite database files tracked by the application, including sidecar files.
    /// </summary>
    public List<string> DatabaseFiles => [DatabasePath, Database_SHM_Path, Database_WAL_Path];

    /// <summary>
    /// Gets the root directory for user media data, where default destination folders such as Movies, TV shows, Music, and Downloads are created.
    /// This is distinct from <see cref="ConfigDirectory"/>, which stores Reaparr's internal application files rather than media content.
    /// </summary>
    public string DataDirectory
    {
        get
        {
            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return dataPath;

            if (EnvironmentExtensions.IsDockerMode())
                return "/";

            if (EnvironmentExtensions.IsDesktopMode())
                return Path.Combine(UserDirectory.DownloadDir, DefaultReaparrFolderName);

            throw new PlatformNotSupportedException($"Platform: {OsInfo.CurrentOS} is not supported");
        }
    }

    #region Interface Implementations

    /// <summary>
    /// Gets the root directory where Reaparr stores downloaded and managed media.
    /// </summary>
    string IPathProvider.RootDirectory => DataDirectory;

    /// <summary>
    /// Gets the full path to the main Reaparr settings file.
    /// </summary>
    string IPathProvider.ConfigFileLocation => ConfigFileLocation;

    /// <summary>
    /// Gets the file name used for the main Reaparr settings file.
    /// </summary>
    string IPathProvider.ConfigFileName => ConfigFileName;

    /// <summary>
    /// Gets the directory used to store database backup files.
    /// </summary>
    string IPathProvider.DatabaseBackupDirectory => DatabaseBackupDirectory;

    /// <summary>
    /// Gets the file name used for the SQLite database.
    /// </summary>
    string IPathProvider.DatabaseName => DatabaseName;

    /// <summary>
    /// Gets the full path to the SQLite database file.
    /// </summary>
    string IPathProvider.DatabasePath => DatabasePath;

    /// <summary>
    /// Gets the directory used to store application log files.
    /// </summary>
    string IPathProvider.LogsDirectory => LogsDirectory;

    /// <summary>
    /// Gets the resolved configuration directory used by the application.
    /// </summary>
    string IPathProvider.ConfigDirectory => ConfigDirectory;

    #endregion

    #endregion
}
