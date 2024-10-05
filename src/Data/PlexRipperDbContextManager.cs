using System.Globalization;
using Data.Contracts;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;

namespace PlexRipper.Data;

public class PlexRipperDbContextManager : IPlexRipperDbContextManager
{
    private readonly ILog<PlexRipperDbContextManager> _log;

    private readonly IPlexRipperDbContextDatabase _dbContextDatabase;

    private readonly IPathProvider _pathProvider;

    private readonly IFileSystem _fileSystem;

    private readonly IDirectorySystem _directorySystem;

    private string DatabaseName => _pathProvider.DatabaseName;
    private string DatabasePath => _pathProvider.DatabasePath;

    private string ConfigDirectory => _pathProvider.ConfigDirectory;

    public PlexRipperDbContextManager(
        ILog<PlexRipperDbContextManager> log,
        IPlexRipperDbContextDatabase dbContextDatabaseDatabase,
        IPathProvider pathProvider,
        IFileSystem fileSystem,
        IDirectorySystem directorySystem
    )
    {
        _log = log;
        _dbContextDatabase = dbContextDatabaseDatabase;
        _pathProvider = pathProvider;
        _fileSystem = fileSystem;
        _directorySystem = directorySystem;
    }

    public Result Setup()
    {
        if (EnvironmentExtensions.IsIntegrationTestMode())
        {
            _log.InformationLine("Integration test mode detected, skipping database setup");
            return Result.Ok();
        }

        if (_fileSystem.FileExists(DatabasePath))
        {
            // Check if database can be connected to.
            if (_dbContextDatabase.CanConnect())
            {
                _log.InformationLine("Database was successfully connected!");
                _log.Information("Database connected at: {DatabasePath}", DatabasePath);

                MigrateDatabase();

                return Result.Ok();
            }

            _log.Error(
                "Database exists at {DatabasePath} but could not be connected to, resetting database now",
                DatabasePath
            );
            return ResetDatabase();
        }

        _log.WarningLine("Database does not exist, creating a new one now");

        _dbContextDatabase.EnsureCreated();
        return MigrateDatabase();
    }

    public Result ResetDatabase()
    {
        try
        {
            _log.InformationLine("Resetting PlexRipper database now");
            _dbContextDatabase.CloseConnection();
            BackUpDatabase();

            var deletedResult = _dbContextDatabase.EnsureDeleted();
            if (deletedResult.IsFailed)
                return deletedResult.LogError();

            var createdResult = _dbContextDatabase.EnsureCreated();
            if (createdResult.IsFailed)
            {
                _log.ErrorLine("Database could not be created at {DatabasePath}", DatabasePath);
                return createdResult.LogError();
            }

            MigrateDatabase();
            return Result.Ok();
        }
        catch (Exception e)
        {
            _log.FatalLine("Failed to reset database!");
            _log.FatalLine("TO FIX THIS: DELETE DATABASE MANUALLY FROM THE CONFIG DIRECTORY");
            return Result.Fail(new ExceptionalError(e)).LogFatal();
        }
    }

    private Result MigrateDatabase()
    {
        try
        {
            // Don't migrate when running in memory, this causes error:
            // "Relational-specific methods can only be used when the context is using a relational database provider."
            if (!_dbContextDatabase.IsInMemory() && _dbContextDatabase.GetPendingMigrations().Any())
            {
                _log.InformationLine("Attempting to migrate database");
                _dbContextDatabase.Migrate();
                _log.InformationLine("Database migration successful!");
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            _log.ErrorLine("Failed to migrate the database or the database is corrupted");
            _log.Error(e);

            return Result.Fail(new ExceptionalError(e)).LogFatal();
        }
    }

    private Result BackUpDatabase()
    {
        _log.InformationLine("Attempting to back-up the PlexRipper database");
        if (!_fileSystem.FileExists(_pathProvider.DatabasePath))
        {
            _log.InformationLine("Database does not exist, cannot continue to back-up");
            return Result.Ok();
        }

        var dateString = DateTime.UtcNow.ToString("yy-MM-dd_hh-mm", CultureInfo.InvariantCulture);
        var dbBackUpPath = Path.Combine(_pathProvider.DatabaseBackupDirectory, dateString);

        try
        {
            _directorySystem.CreateDirectory(dbBackUpPath);

            // Wait until the database is available.
            StreamExtensions
                .WaitForFile(_pathProvider.DatabasePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                ?.Dispose();

            foreach (var databaseFilePath in _pathProvider.DatabaseFiles)
            {
                if (_fileSystem.FileExists(databaseFilePath))
                {
                    var destinationPath = Path.Combine(dbBackUpPath, databaseFilePath.GetFileName());
                    try
                    {
                        _fileSystem.Copy(databaseFilePath, destinationPath);
                        _log.Here()
                            .Information(
                                "Successfully copied \"{DatabaseFilePath}\" to back-up location\"{DestinationPath}\"",
                                databaseFilePath,
                                destinationPath
                            );
                    }
                    catch (Exception e)
                    {
                        _log.Here()
                            .Error(
                                "Failed to copy {DatabaseFilePath} to back-up location {DestinationPath}",
                                databaseFilePath,
                                destinationPath
                            );
                        _log.Error(e);
                    }

                    continue;
                }

                _log.Warning("Could not find: {DatabaseFilePath} to backup", databaseFilePath);
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
