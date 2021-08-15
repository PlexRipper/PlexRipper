using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.FileSystem
{
    public class FileSystem : System.IO.Abstractions.FileSystem, IFileSystem
    {
        #region Fields

        private readonly IDiskProvider _diskProvider;

        #endregion

        #region Constructor

        public FileSystem(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        #endregion

        #region Properties

        public string ConfigDirectory => Path.Combine(RootDirectory, "config");

        public string DatabaseName => EnviromentExtensions.IsIntegrationTestMode() ? "PlexRipperDB_Tests.db" : "PlexRipperDB.db";

        public string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

        public string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

        public string LogsDirectory => Path.Combine(RootDirectory, "config", "logs");

        public string ConfigFileName => "PlexRipperSettings.json";

        public string ConfigFileLocation => Path.Join(ConfigDirectory, ConfigFileName);

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
                        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                    default:
                        return "/";
                }
            }
        }

        #endregion

        #region Public Methods

        public long CheckDirectoryAvailableSpace(string directory)
        {
            var root = GetPathRoot(directory);
            var drive = new DriveInfo(root);
            return drive.AvailableFreeSpace;
        }

        public Result CreateDirectoryFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            {
                return Result.Fail("parameter filepath was empty");
            }

            var directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;
            if (string.IsNullOrEmpty(directoryPath))
            {
                return Result.Fail($"Could not determine the directory name of path: {filePath}");
            }

            Directory.CreateDirectory(directoryPath);
            return Result.Ok();
        }

        /// <inheritdoc />
        public Result DeleteAllFilesFromDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                var di = new DirectoryInfo(directory);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                return Result.Ok();
            }

            return Result.Fail($"Directory: {directory} does not exist").LogError();
        }

        /// <inheritdoc />
        public Result DeleteDirectoryFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            {
                return Result.Fail("Parameter filepath was empty").LogError();
            }

            try
            {
                var directory = Path.GetDirectoryName(filePath) ?? string.Empty;

                // If the filePath is just an empty directory then delete that.
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory) && !Directory.GetFiles(directory).Any())
                {
                    Directory.Delete(directory);
                }
                else
                {
                    return Result.Fail($"Could not determine the directory name of path: {filePath} or the path contains files").LogError();
                }
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }

            return Result.Ok();
        }

        public Result<Stream> DownloadWorkerTempFileStream(string directory, string fileName, long fileSize)
        {
            try
            {
                Directory.CreateDirectory(directory);
                var availableSpace = CheckDirectoryAvailableSpace(directory);
                if (availableSpace < fileSize)
                {
                    return Result.Fail(
                        $"There is not enough space available in root directory {directory}");
                }

                var filePath = Path.Combine(directory, fileName);

                Stream fileStream;
                if (File.Exists(filePath))
                {
                    fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete);
                }
                else
                {
                    fileStream = File.Create(filePath, 2048, FileOptions.Asynchronous);
                }

                // var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
                // Pre-allocate the required file size
                fileStream.SetLength(fileSize);
                return Result.Ok(fileStream);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public string GetPathRoot(string directory)
        {
            var f = new FileInfo(directory);
            return Path.GetPathRoot(f.FullName);
        }

        public Result<FileSystemResult> LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes)
        {
            Log.Debug("Looking up path: {query}");

            // If path is invalid return root file system
            if (string.IsNullOrWhiteSpace(query) || !Directory.Exists(query))
            {
                return Result.Ok(new FileSystemResult
                {
                    Directories = GetDrives(),
                });
            }

            if (allowFoldersWithoutTrailingSlashes)
            {
                return Result.Ok(GetResult(query, includeFiles));
            }

            var lastSeparatorIndex = query.LastIndexOf(Path.DirectorySeparatorChar);
            var path = query.Substring(0, lastSeparatorIndex + 1);

            if (lastSeparatorIndex != -1)
            {
                return Result.Ok(GetResult(path, includeFiles));
            }

            return Result.Ok(new FileSystemResult());
        }

        public Result<Stream> SaveFile(string directory, string fileName, long fileSize)
        {
            try
            {
                var fullPath = Path.Combine(directory, fileName);
                if (Directory.Exists(fullPath))
                {
                    Log.Warning($"Path: {fullPath} already exists, will overwrite now");
                }

                Directory.CreateDirectory(directory);

                var availableSpace = CheckDirectoryAvailableSpace(directory);

                if (availableSpace < fileSize)
                {
                    return Result.Fail(
                        $"There is not enough space available in root directory {directory}");
                }

                Stream fileStream = File.Create(fullPath, 4096, FileOptions.Asynchronous);

                // Pre-allocate the required file size
                fileStream.SetLength(fileSize);

                return Result.Ok(fileStream);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public Result Setup()
        {
            Log.Information("Setting up File System Service");
            return CreateConfigDirectory();
        }

        public string ToAbsolutePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(RootDirectory, relativePath));
        }

        #endregion

        #region Private Methods

        private Result CreateConfigDirectory()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                {
                    Log.Debug("Config directory doesn't exist, will create now.");

                    Directory.CreateDirectory(ConfigDirectory);

                    Log.Debug($"Directory: \"{ConfigDirectory}\" created!");
                }
                else
                {
                    Log.Debug("Config directory exists!");
                }

                return Result.Ok();
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                return Result.Fail(new ExceptionalError(e));
            }
        }

        private List<FileSystemModel> GetDrives()
        {
            return _diskProvider.GetMounts()
                .Select(d => new FileSystemModel
                {
                    Type = FileSystemEntityType.Drive,
                    Name = _diskProvider.GetVolumeName(d),
                    Path = d.RootDirectory,
                    LastModified = null,
                })
                .ToList();
        }

        private FileSystemResult GetResult(string path, bool includeFiles)
        {
            var result = new FileSystemResult();

            try
            {
                result.Parent = _diskProvider.GetParent(path);
                result.Directories = _diskProvider.GetDirectories(path);

                if (includeFiles)
                {
                    result.Files = _diskProvider.GetFiles(path);
                }
            }
            catch (DirectoryNotFoundException)
            {
                return new FileSystemResult { Parent = _diskProvider.GetParent(path) };
            }
            catch (ArgumentException)
            {
                return new FileSystemResult();
            }
            catch (IOException)
            {
                return new FileSystemResult { Parent = _diskProvider.GetParent(path) };
            }
            catch (UnauthorizedAccessException)
            {
                return new FileSystemResult { Parent = _diskProvider.GetParent(path) };
            }

            return result;
        }

        #endregion
    }
}