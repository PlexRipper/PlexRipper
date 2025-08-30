using System.Globalization;
using System.IO.Abstractions;
using Reaparr.Data.Contracts;
using Reaparr.Environment;
using Reaparr.Identity.Contracts;
using Reaparr.Logging;
using Reaparr.Logging.Interface;
using Reaparr.Settings.Contracts;

namespace Reaparr.Data;

public class PlexRipperDbContextManager : IPlexRipperDbContextManager
{
    private readonly ILog<PlexRipperDbContextManager> _log;

    private readonly IPlexRipperDbContextDatabase _plexRipperDbContextDatabase;
    private readonly IAuthDbContextDatabase _authDbContextDatabase;
    private readonly IGeneralSettings _generalSettings;

    private readonly IPathProvider _pathProvider;

    private readonly IDirectory _directory;
    private readonly IFile _file;
    private string DatabasePath => _pathProvider.DatabasePath;

    public PlexRipperDbContextManager(
        ILog<PlexRipperDbContextManager> log,
        IPlexRipperDbContextDatabase plexRipperDbContextDatabase,
        IAuthDbContextDatabase authDbContextDatabase,
        IGeneralSettings generalSettings,
        IPathProvider pathProvider,
        IDirectory directory,
        IFile file
    )
    {
        _log = log;
        _plexRipperDbContextDatabase = plexRipperDbContextDatabase;
        _authDbContextDatabase = authDbContextDatabase;
        _generalSettings = generalSettings;
        _pathProvider = pathProvider;
        _directory = directory;
        _file = file;
    }

    public Result Setup()
    {
        if (EnvironmentExtensions.IsIntegrationTestMode())
        {
            _log.InformationLine("Integration test mode detected, skipping database setup");
            return Result.Ok();
        }

        if (_file.Exists(DatabasePath))
        {
            // Check if the database can be connected to.
            if (_plexRipperDbContextDatabase.CanConnect())
            {
                _log.InformationLine("Database was successfully connected!");
                _log.Information("Database connected at: {DatabasePath}", DatabasePath);

                return MigrateDatabase();
            }

            _log.Error(
                "Database exists at {DatabasePath} but could not be connected to, resetting database now",
                DatabasePath
            );
            return ResetDatabase();
        }

        _log.WarningLine("Database does not exist, creating a new one now");

        return CreateDatabase();
    }

    public Result ResetDatabase()
    {
        try
        {
            _log.InformationLine("Resetting PlexRipper database now");
            _plexRipperDbContextDatabase.CloseConnection();

            var backUpResult = BackUpDatabase();
            if (backUpResult.IsFailed)
            {
                _log.ErrorLine("Failed to back-up database");
                return backUpResult.LogError();
            }

            var deletedResult = _plexRipperDbContextDatabase.EnsureDeleted();
            if (deletedResult.IsFailed)
            {
                _log.Error("Database could not be deleted at {DatabasePath}", DatabasePath);
                return deletedResult.LogError();
            }

            if (deletedResult.Value)
                _log.Warning("Database was successfully deleted at: {DatabasePath}", DatabasePath);

            var createdResult = CreateDatabase();
            if (createdResult.IsFailed)
            {
                _log.Error("Database could not be created at {DatabasePath}", DatabasePath);
                return createdResult.LogError();
            }

            _generalSettings.FirstTimeSetup = true;
            _log.InformationLine("First time setup has been set to true because the database has been reset");

            return Result.Ok();
        }
        catch (Exception e)
        {
            _log.FatalLine("Failed to reset database!");
            _log.FatalLine("TO FIX THIS: DELETE DATABASE MANUALLY FROM THE CONFIG DIRECTORY");
            return Result.Fail(new ExceptionalError(e)).LogFatal();
        }
    }

    private Result CreateDatabase()
    {
        try
        {
            // Create the database while applying any pending migrations.
            _plexRipperDbContextDatabase.Migrate();
            _authDbContextDatabase.Migrate();
            _log.Information("The new database was successfully created at: {DatabasePath}", DatabasePath);
            return Result.Ok();
        }
        catch (Exception e)
        {
            _log.ErrorLine("Failed to create the database");
            _log.Error(e);

            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    private Result MigrateDatabase()
    {
        try
        {
            // Don't migrate when running in memory, this causes an error:
            // "Relational-specific methods can only be used when the context is using a relational database provider."
            var pendingMigrations = _plexRipperDbContextDatabase.GetPendingMigrations();
            if (!_plexRipperDbContextDatabase.IsInMemory() && pendingMigrations.Any())
            {
                _log.InformationLine("Attempting to migrate database, this might take a while");
                var migrateResult = _plexRipperDbContextDatabase.Migrate();
                if (migrateResult.IsFailed)
                {
                    _log.ErrorLine("Failed to migrate the database");
                    migrateResult.LogError();
                    ResetDatabase();
                }
                else
                {
                    _log.InformationLine("Database migration successful!");
                }
            }

            pendingMigrations = _authDbContextDatabase.GetPendingMigrations();
            if (!_authDbContextDatabase.IsInMemory() && pendingMigrations.Any())
            {
                _log.InformationLine("Attempting to migrate Authentication tables database");
                var migrateResult = _authDbContextDatabase.Migrate();
                if (migrateResult.IsFailed)
                {
                    _log.ErrorLine("Failed to migrate Authentication tables database");
                    migrateResult.LogError();
                    ResetDatabase();
                }
                else
                {
                    _log.InformationLine("Authentication tables migration successful!");
                }
            }

            return Result.Ok();
        }
        catch (Exception e)
        {
            _log.ErrorLine("Failed to migrate the database or the database is corrupted");
            _log.Error(e);

            return ResetDatabase();
        }
    }

    private Result BackUpDatabase()
    {
        _log.InformationLine("Attempting to back-up the PlexRipper database");
        if (!_file.Exists(_pathProvider.DatabasePath))
        {
            _log.InformationLine("Database does not exist, cannot continue to back-up");
            return Result.Ok();
        }

        var dateString = DateTime.UtcNow.ToString("yy-MM-dd_hh-mm", CultureInfo.InvariantCulture);
        var dbBackUpPath = Path.Combine(_pathProvider.DatabaseBackupDirectory, dateString);

        var createDirectoryResult = Result.Try(() => _directory.CreateDirectory(dbBackUpPath));
        if (createDirectoryResult.IsFailed)
        {
            _log.Error("Failed to create back-up directory at {DbBackUpPath}", dbBackUpPath);
            return createDirectoryResult.LogError();
        }

        // Wait until the database is available.
        StreamExtensions
            .WaitForFile(_pathProvider.DatabasePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            ?.Dispose();

        foreach (var databaseFilePath in _pathProvider.DatabaseFiles)
        {
            if (_file.Exists(databaseFilePath))
            {
                var combineResult = Result.Try(() => Path.Combine(dbBackUpPath, databaseFilePath.GetFileName()));
                if (combineResult.IsFailed)
                    return combineResult.LogError();

                var destinationPath = combineResult.Value;

                var copyResult = Result.Try((() => _file.Copy(databaseFilePath, destinationPath)));
                if (copyResult.IsFailed)
                {
                    _log.Here()
                        .Error(
                            "Failed to copy {DatabaseFilePath} to back-up location {DestinationPath}",
                            databaseFilePath,
                            destinationPath
                        );
                    return copyResult.LogError();
                }

                _log.Here()
                    .Information(
                        "Successfully copied \"{DatabaseFilePath}\" to back-up location\"{DestinationPath}\"",
                        databaseFilePath,
                        destinationPath
                    );

                continue;
            }

            _log.Warning("Could not find: {DatabaseFilePath} to backup", databaseFilePath);
        }

        return Result.Ok();
    }
}
