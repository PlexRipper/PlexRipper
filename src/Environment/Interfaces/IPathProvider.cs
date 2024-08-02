namespace Environment;

public interface IPathProvider
{
    string ConfigDirectory { get; }

    string ConfigFileLocation { get; }

    string ConfigFileName { get; }

    string DatabaseBackupDirectory { get; }

    string DatabaseName { get; }

    string DatabasePath { get; }

    string Database_SHM_Path { get; }

    string Database_WAL_Path { get; }

    string LogsDirectory { get; }

    string RootDirectory { get; }

    List<string> DatabaseFiles { get; }
}
