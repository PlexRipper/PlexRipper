using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Environment;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class PlexRipperDatabaseService : IPlexRipperDatabaseService
    {
        private readonly PlexRipperDbContext _dbContext;

        private readonly IPathProvider _pathProvider;

        private readonly IFileSystem _fileSystem;

        public PlexRipperDatabaseService(PlexRipperDbContext dbContext, IPathProvider pathProvider, IFileSystem fileSystem)
        {
            _dbContext = dbContext;
            _pathProvider = pathProvider;
            _fileSystem = fileSystem;
        }

        public Result BackUpDatabase()
        {
            Log.Information("Attempting to back-up the PlexRipper database");
            if (!_fileSystem.FileExists(_pathProvider.DatabasePath))
            {
                return Result.Fail($"Could not find Database at path: {_pathProvider.DatabasePath}").LogError();
            }

            var dbBackupName = $"BackUp_{_pathProvider.DatabaseName.Replace(".db", "")}_" +
                               $"{DateTime.Now.ToString("dd-MM-yyyy_hh-mm", CultureInfo.InvariantCulture)}.db";
            var dbBackUpPath = Path.Combine(_pathProvider.DatabaseBackupDirectory, dbBackupName);

            try
            {
                _fileSystem.CreateDirectoryFromFilePath(dbBackUpPath);

                // Wait until the database is available.
                StreamExtensions.WaitForFile(_pathProvider.DatabasePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)?.Dispose();

                var moveResult = _fileSystem.FileMove(_pathProvider.DatabasePath, dbBackUpPath);
                if (moveResult.IsFailed)
                {
                    return moveResult;
                }
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }

            Log.Information($"Successfully backed-up the database to {dbBackUpPath}");
            return Result.Ok();
        }

        public async Task<Result> ResetDatabase()
        {
            Log.Information("Resetting PlexRipper database");
            var backupResult = BackUpDatabase();
            if (backupResult.IsFailed)
            {
                return backupResult;
            }

            return await _dbContext.SetupAsync();
        }

        public async Task<Result> SetupAsync()
        {
            Log.Information("Setting up the PlexRipper database");
            var setupResult = await _dbContext.SetupAsync();
            if (setupResult.IsSuccess)
            {
                return setupResult;
            }

            Log.Warning("Failed to setup the database, will back-up and reset now.");
            return await ResetDatabase();
        }
    }
}