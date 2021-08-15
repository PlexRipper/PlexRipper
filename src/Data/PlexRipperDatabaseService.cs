using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.Data
{
    public class PlexRipperDatabaseService : IPlexRipperDatabaseService
    {
        private readonly PlexRipperDbContext _dbContext;

        private readonly IFileSystem _fileSystem;

        public PlexRipperDatabaseService(PlexRipperDbContext dbContext, IFileSystem fileSystem)
        {
            _dbContext = dbContext;
            _fileSystem = fileSystem;
            _dbContext.DatabaseName = _fileSystem.DatabaseName;
            _dbContext.DatabasePath = _fileSystem.DatabasePath;
            _dbContext.ConfigDirectory = _fileSystem.ConfigDirectory;
        }

        public Result BackUpDatabase()
        {
            Log.Information("Attempting to back-up the PlexRipper database");
            if (!File.Exists(_fileSystem.DatabasePath))
            {
                return Result.Fail($"Could not find Database at path: {_fileSystem.DatabasePath}").LogError();
            }

            var dbBackupName = $"BackUp_{_fileSystem.DatabaseName.Replace(".db", "")}_" +
                               $"{DateTime.Now.ToString("dd-MM-yyyy_hh-mm", CultureInfo.InvariantCulture)}.db";
            var dbBackUpPath = Path.Combine(_fileSystem.DatabaseBackupDirectory, dbBackupName);

            try
            {
                _fileSystem.CreateDirectoryFromFilePath(dbBackUpPath);

                // Wait until the database is available.
                StreamExtensions.WaitForFile(_fileSystem.DatabasePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)?.Dispose();

                File.Move(_fileSystem.DatabasePath, dbBackUpPath);
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