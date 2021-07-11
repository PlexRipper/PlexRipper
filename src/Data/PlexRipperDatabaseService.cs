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
        }

        public Result BackUpDatabase()
        {
            if (!File.Exists(_dbContext.DatabasePath))
            {
                return Result.Fail($"Could not find Database at path: {_dbContext.DatabasePath}");
            }

            try
            {
                var dbBackupName = $"BackUp_{PlexRipperDbContext.DatabaseName.Replace(".db", "")}_" +
                                   $"{DateTime.Now.ToString("dd-MM-yyyy_hh-mm", CultureInfo.InvariantCulture)}.db";
                var dbBackUpPath = Path.Combine(FileSystemPaths.DatabaseBackupDirectory, dbBackupName);

                _fileSystem.CreateDirectoryFromFilePath(dbBackUpPath);

                File.Move(_dbContext.DatabasePath, dbBackUpPath);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }

            return Result.Ok();
        }

        public async Task<Result> ResetDatabase()
        {
            var backupResult = BackUpDatabase();
            if (backupResult.IsFailed)
            {
                return backupResult;
            }

            return await _dbContext.SetupAsync();
        }

        public async Task<Result> SetupAsync()
        {
            var setupResult =  await _dbContext.SetupAsync();
            if (setupResult.IsSuccess)
            {
                return setupResult;
            }

            return await ResetDatabase();
        }
    }
}