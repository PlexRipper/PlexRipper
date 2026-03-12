using System.Reflection;

namespace Reaparr.Environment;

public class PathProvider : IPathProvider
{
    #region Properties

    #region DirectoryNames

    private static readonly string _configFolder = "Config";

    private static readonly string _logsFolder = "Logs";

    public static string DefaultMovieFolderName => "Movies";

    public static string DefaultDownloadsFolderName => "Downloads";

    public static string DefaultTvShowsFolderName => "TvShows";

    public static string DefaultMusicFolderName => "Music";

    public static string DefaultPhotosFolderName => "Photos";

    public static string DefaultOtherFolderName => "Other";

    public static string DefaultGamesFolderName => "Games";

    public static string DefaultMovieDestinationFolder => Path.Combine(RootDirectory, DefaultMovieFolderName);

    public static string DefaultDownloadsDestinationFolder => Path.Combine(RootDirectory, DefaultDownloadsFolderName);

    public static string DefaultTvShowsDestinationFolder => Path.Combine(RootDirectory, DefaultTvShowsFolderName);

    public static string DefaultMusicDestinationFolder => Path.Combine(RootDirectory, DefaultMusicFolderName);

    public static string DefaultPhotosDestinationFolder => Path.Combine(RootDirectory, DefaultPhotosFolderName);

    public static string DefaultOtherDestinationFolder => Path.Combine(RootDirectory, DefaultOtherFolderName);

    public static string DefaultGamesDestinationFolder => Path.Combine(RootDirectory, DefaultGamesFolderName);

    #endregion

    #region FileNames

    public static string ConfigFileName => "ReaparrSettings.json";

    public static string DatabaseName => "ReaparrDB.db";
    public static string DatabaseShmName => $"{DatabaseName}-shm";
    public static string DatabaseWalName => $"{DatabaseName}-wal";

    #endregion

    public static string ConfigDirectory => Path.Combine(RootDirectory, _configFolder);

    public static string ConfigFileLocation => Path.Join(ConfigDirectory, ConfigFileName);

    public static string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

    public static string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

    // ReSharper disable once InconsistentNaming
    public static string Database_SHM_Path => Path.Combine(ConfigDirectory, DatabaseShmName);

    // ReSharper disable once InconsistentNaming
    public static string Database_WAL_Path => Path.Combine(ConfigDirectory, DatabaseWalName);

    public static string LogsDirectory => Path.Combine(RootDirectory, _configFolder, _logsFolder);

    public List<string> DatabaseFiles => [DatabasePath, Database_SHM_Path, Database_WAL_Path];

    public static string RootDirectory
    {
        get
        {
            var devRootPath = EnvironmentExtensions.GetDevelopmentRootPath();
            if (devRootPath is not null)
                return devRootPath;

            switch (OsInfo.CurrentOS)
            {
                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.Osx:
                    return "/";
                case OperatingSystemPlatform.Windows:
                    return Path.GetPathRoot(Assembly.GetExecutingAssembly().Location) ?? @"C:\";
                default:
                    return "/";
            }
        }
    }

    #region Interface Implementations

    string IPathProvider.RootDirectory => RootDirectory;

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
