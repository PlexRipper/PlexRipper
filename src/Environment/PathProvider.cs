using System.Reflection;

namespace Reaparr.Environment;

public class PathProvider : IPathProvider
{
    #region Properties

    #region DirectoryNames

    public static readonly string DefaultConfigFolderName = "Config";

    private static readonly string _logsFolder = "Logs";

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

    /// <summary>
    /// Gets the default movies destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public static string DefaultMovieDestinationFolder => GetDefaultMediaDirectory(DefaultMovieFolderName);

    /// <summary>
    /// Gets the default downloads destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public static string DefaultDownloadsDestinationFolder => GetDefaultMediaDirectory(DefaultDownloadsFolderName);

    /// <summary>
    /// Gets the default TV shows destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public static string DefaultTvShowsDestinationFolder => GetDefaultMediaDirectory(DefaultTvShowsFolderName);

    /// <summary>
    /// Gets the default music destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public static string DefaultMusicDestinationFolder => GetDefaultMediaDirectory(DefaultMusicFolderName);

    /// <summary>
    /// Gets the default photos destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public static string DefaultPhotosDestinationFolder => GetDefaultMediaDirectory(DefaultPhotosFolderName);

    /// <summary>
    /// Gets the default uncategorized media destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public static string DefaultOtherDestinationFolder => GetDefaultMediaDirectory(DefaultOtherFolderName);

    /// <summary>
    /// Gets the default games destination path based on the configured data root or the current platform fallback.
    /// </summary>
    public static string DefaultGamesDestinationFolder => GetDefaultMediaDirectory(DefaultGamesFolderName);

    #endregion

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
                return Path.Combine(GetDockerRootDirectory(), DefaultConfigFolderName);

            // Desktop mode
            return OsInfo.CurrentOS switch
            {
                OperatingSystemPlatform.Windows => Path.Combine(GetAppDataDirectory(), "Reaparr"),
                OperatingSystemPlatform.Osx => Path.Combine(
                    GetHomeDirectory(),
                    "Library",
                    "Application Support",
                    "Reaparr"
                ),
                _ => Path.Combine(GetHomeDirectory(), ".config", "Reaparr"),
            };
        }
    }

    /// <summary>
    /// Gets the full path to the main Reaparr settings file.
    /// </summary>
    public static string ConfigFileLocation => Path.Join(ConfigDirectory, ConfigFileName);

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
    public static string DataDirectory
    {
        get
        {
            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return dataPath;

            return EnvironmentExtensions.IsDockerMode() ? GetDockerRootDirectory() : GetHomeDirectory();
        }
    }

    private static string GetDefaultMediaDirectory(string folderName)
    {
        var dataPath = EnvironmentExtensions.GetDataPath();
        if (dataPath is not null)
            return Path.Combine(dataPath, folderName);

        if (EnvironmentExtensions.IsDockerMode())
            return Path.Combine(GetDockerRootDirectory(), folderName);

        return Path.Combine(GetHomeDirectory(), folderName);
    }

    private static string GetAppDataDirectory() =>
        System.Environment.GetEnvironmentVariable("APPDATA")
        ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

    private static string GetHomeDirectory() =>
        System.Environment.GetEnvironmentVariable("HOME")
        ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

    private static string GetDockerRootDirectory() =>
        OsInfo.CurrentOS switch
        {
            OperatingSystemPlatform.Windows => Path.GetPathRoot(Assembly.GetExecutingAssembly().Location) ?? @"C:",
            _ => "/",
        };

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
