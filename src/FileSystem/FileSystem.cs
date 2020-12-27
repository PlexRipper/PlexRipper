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
    public class FileSystem : IFileSystem
    {
        #region Fields

        private readonly IDiskProvider _diskProvider;

        #endregion Fields

        #region Constructors

        public FileSystem(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        #endregion Constructors

        #region Properties

        public string RootDirectory
        {
            get
            {
                switch (OsInfo.Os)
                {
                    case Os.Linux:
                    case Os.Osx:
                        return "/";
                    case Os.Windows:
                        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                    default:
                        return "/";
                }
            }
        }

        public string ConfigDirectory => Path.Combine(RootDirectory, "config");

        #endregion Properties

        #region Methods

        public Result Setup()
        {
            return CreateConfigDirectory();
        }

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

        public Result CreateDirectoryFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            {
                return Result.Fail("parameter filepath was empty");
            }

            string directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;
            if (string.IsNullOrEmpty(directoryPath))
            {
                return Result.Fail($"Could not determine the directory name of path: {filePath}");
            }

            Directory.CreateDirectory(directoryPath);
            return Result.Ok();
        }

        /// <inheritdoc/>
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

        public static string GetPathRoot(string directory)
        {
            FileInfo f = new FileInfo(directory);
            return Path.GetPathRoot(f.FullName);
        }

        public static long CheckDirectoryAvailableSpace(string directory)
        {
            var root = GetPathRoot(directory);
            DriveInfo drive = new DriveInfo(root);
            return drive.AvailableFreeSpace;
        }

        public FileSystemResult LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes)
        {
            // If path is invalid return root file system
            if (string.IsNullOrWhiteSpace(query) || !Directory.Exists(query))
            {
                return new FileSystemResult
                {
                    Directories = GetDrives(),
                };
            }

            if (allowFoldersWithoutTrailingSlashes)
            {
                return GetResult(query, includeFiles);
            }

            var lastSeparatorIndex = query.LastIndexOf(Path.DirectorySeparatorChar);
            var path = query.Substring(0, lastSeparatorIndex + 1);

            if (lastSeparatorIndex != -1)
            {
                return GetResult(path, includeFiles);
            }

            return new FileSystemResult();
        }

        public Result<FileStream> SaveFile(string directory, string fileName, long fileSize)
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

                var fileStream = File.Create(fullPath, 4096, FileOptions.Asynchronous);

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

        public Result<FileStream> DownloadWorkerTempFileStream(string directory, string fileName, long fileSize)
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

                FileStream fileStream;
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

        /// <inheritdoc/>
        public Result DeleteAllFilesFromDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                DirectoryInfo di = new DirectoryInfo(directory);

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

        public string ToAbsolutePath(string relativePath)
        {
            var x = Path.GetFullPath(Path.Combine(RootDirectory, relativePath));
            return x;
        }

        #endregion Methods
    }
}