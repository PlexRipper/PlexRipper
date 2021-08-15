
namespace PlexRipper.Application.Common
{
    public interface IPathSystem
    {
        string ConfigDirectory { get; }

        string ConfigFileLocation { get; }

        string ConfigFileName { get; }

        string DatabaseBackupDirectory { get; }

        string DatabaseName { get; }

        string DatabasePath { get; }

        string LogsDirectory { get; }

        string RootDirectory { get; }
    }
}