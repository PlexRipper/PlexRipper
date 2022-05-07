using System.IO;
using System.Reflection;

namespace Environment
{
    public class PathProvider : IPathProvider
    {
        #region Properties

        public static string ConfigDirectory => Path.Combine(RootDirectory, "config");

        public static string ConfigFileLocation => Path.Join(ConfigDirectory, ConfigFileName);

        public static string ConfigFileName => "PlexRipperSettings.json";

        public static string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

        public static string DatabaseName => "PlexRipperDB.db";

        public static string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

        public static string LogsDirectory => Path.Combine(RootDirectory, "config", "logs");

        public static string RootDirectory
        {
            get
            {
                var devRootPath = EnvironmentExtensions.GetDevelopmentRootPath();
                if (devRootPath is not null)
                {
                    return devRootPath;
                }

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
}