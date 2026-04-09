using System.Reflection;

namespace Reaparr.Environment;

public class PathProvider : IPathProvider
{
    #region Properties

    #region DirectoryNames

    public static readonly string DefaultConfigFolderName = "Config";

    private static readonly string _logsFolder = "Logs";

    public static string DefaultMovieFolderName => "Movies";

    public static string DefaultDownloadsFolderName => "Downloads";

    public static string DefaultTvShowsFolderName => "TvShows";

    public static string DefaultMusicFolderName => "Music";

    public static string DefaultPhotosFolderName => "Photos";

    public static string DefaultOtherFolderName => "Other";

    public static string DefaultGamesFolderName => "Games";

    public static string DefaultMovieDestinationFolder => GetDefaultMediaDirectory(DefaultMovieFolderName);

    public static string DefaultDownloadsDestinationFolder => GetDefaultMediaDirectory(DefaultDownloadsFolderName);

    public static string DefaultTvShowsDestinationFolder => GetDefaultMediaDirectory(DefaultTvShowsFolderName);

    public static string DefaultMusicDestinationFolder => GetDefaultMediaDirectory(DefaultMusicFolderName);

    public static string DefaultPhotosDestinationFolder => GetDefaultMediaDirectory(DefaultPhotosFolderName);

    public static string DefaultOtherDestinationFolder => GetDefaultMediaDirectory(DefaultOtherFolderName);

    public static string DefaultGamesDestinationFolder => GetDefaultMediaDirectory(DefaultGamesFolderName);

    #endregion

    #region FileNames

    public static string ConfigFileName => "ReaparrSettings.json";

    public static string DatabaseName => "ReaparrDB.db";
    public static string DatabaseShmName => $"{DatabaseName}-shm";
    public static string DatabaseWalName => $"{DatabaseName}-wal";

    #endregion

    public static string ConfigDirectory => EnvironmentExtensions.GetConfigPath() ?? GetDefaultConfigDirectory();

    public static string ConfigFileLocation => Path.Join(ConfigDirectory, ConfigFileName);

    public static string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

    public static string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

    // ReSharper disable once InconsistentNaming
    public static string Database_SHM_Path => Path.Combine(ConfigDirectory, DatabaseShmName);

    // ReSharper disable once InconsistentNaming
    public static string Database_WAL_Path => Path.Combine(ConfigDirectory, DatabaseWalName);

    public static string LogsDirectory => Path.Combine(ConfigDirectory, _logsFolder);

    public List<string> DatabaseFiles => [DatabasePath, Database_SHM_Path, Database_WAL_Path];

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

    private static string GetDefaultConfigDirectory() =>
        EnvironmentExtensions.IsDockerMode() ? GetDockerConfigDirectory() : GetDesktopConfigDirectory();

    private static string GetDesktopConfigDirectory() =>
        OsInfo.CurrentOS switch
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

    private static string GetDockerConfigDirectory() => Path.Combine(GetDockerRootDirectory(), DefaultConfigFolderName);

    #region Interface Implementations

    string IPathProvider.RootDirectory => DataDirectory;

    string IPathProvider.ConfigFileLocation => ConfigFileLocation;

    string IPathProvider.ConfigFileName => ConfigFileName;

    string IPathProvider.DatabaseBackupDirectory => DatabaseBackupDirectory;

    string IPathProvider.DatabaseName => DatabaseName;

    string IPathProvider.DatabasePath => DatabasePath;

    string IPathProvider.LogsDirectory => LogsDirectory;

    string IPathProvider.ConfigDirectory => ConfigDirectory;

    #endregion

    #endregion
}
