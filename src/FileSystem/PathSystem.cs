using System.Reflection;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using IFileSystem = System.IO.Abstractions.IFileSystem;

namespace PlexRipper.FileSystem
{
    public class PathSystem : IPathSystem
    {
        #region Fields

        private readonly IFileSystem _fileSystem;

        #endregion

        #region Constructor

        public PathSystem(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public PathSystem() : this(new System.IO.Abstractions.FileSystem()) { }

        #endregion

        #region Properties

        public string ConfigDirectory => _fileSystem.Path.Combine(RootDirectory, "config");

        public string ConfigFileLocation => _fileSystem.Path.Join(ConfigDirectory, ConfigFileName);

        public string ConfigFileName => "PlexRipperSettings.json";

        public string DatabaseBackupDirectory => _fileSystem.Path.Combine(ConfigDirectory, "Database BackUp");

        public string DatabaseName => EnviromentExtensions.IsIntegrationTestMode() ? "PlexRipperDB_Tests.db" : "PlexRipperDB.db";

        public string DatabasePath => _fileSystem.Path.Combine(ConfigDirectory, DatabaseName);

        public string LogsDirectory => _fileSystem.Path.Combine(RootDirectory, "config", "logs");

        public string RootDirectory
        {
            get
            {
                switch (OsInfo.CurrentOS)
                {
                    case OperatingSystemPlatform.Linux:
                    case OperatingSystemPlatform.Osx:
                        return "/";
                    case OperatingSystemPlatform.Windows:
                        return _fileSystem.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                    default:
                        return "/";
                }
            }
        }

        #endregion
    }
}