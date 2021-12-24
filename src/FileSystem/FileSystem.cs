using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Environment;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.FileSystem
{
    public class FileSystem : IFileSystem
    {
        #region Fields

        private readonly IPathProvider _pathProvider;

        private readonly System.IO.Abstractions.IFileSystem _abstractedFileSystem;

        private readonly IDiskProvider _diskProvider;

        #endregion

        #region Constructor

        public FileSystem(IPathProvider pathProvider, System.IO.Abstractions.IFileSystem abstractedFileSystem, IDiskProvider diskProvider)
        {
            _pathProvider = pathProvider;
            _abstractedFileSystem = abstractedFileSystem;
            _diskProvider = diskProvider;
        }

        public FileSystem(IPathProvider pathProvider, IDiskProvider diskProvider) : this(pathProvider, new System.IO.Abstractions.FileSystem(),
            diskProvider) { }

        #endregion

        #region Public Methods

        public Result CreateDirectory(string directory)
        {
            try
            {
                _abstractedFileSystem.Directory.CreateDirectory(directory);
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        public Result<bool> FileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Result.Fail($"path is empty: \"{path}\"");
            }

            try
            {
                return Result.Ok(_abstractedFileSystem.File.Exists(path));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
        }

        public Result<string> FileReadAllText(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Result.Fail($"path is empty: \"{path}\"");
            }

            try
            {
                var text = _abstractedFileSystem.File.ReadAllText(path);
                return Result.Ok(text);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
        }

        public Result FileWriteAllText(string path, string text)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Result.Fail($"path is empty: \"{path}\"");
            }

            try
            {
                _abstractedFileSystem.File.WriteAllText(path, text);
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
        }

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

            var directoryPath = _abstractedFileSystem.Path.GetDirectoryName(filePath) ?? string.Empty;
            if (string.IsNullOrEmpty(directoryPath))
            {
                return Result.Fail($"Could not determine the directory name of path: {filePath}");
            }

            _abstractedFileSystem.Directory.CreateDirectory(directoryPath);
            return Result.Ok();
        }

        /// <inheritdoc />
        public Result DeleteAllFilesFromDirectory(string directory)
        {
            if (_abstractedFileSystem.Directory.Exists(directory))
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
                var directory = _abstractedFileSystem.Path.GetDirectoryName(filePath) ?? string.Empty;

                // If the filePath is just an empty directory then delete that.
                if (!string.IsNullOrEmpty(directory) && _abstractedFileSystem.Directory.Exists(directory) &&
                    !_abstractedFileSystem.Directory.GetFiles(directory).Any())
                {
                    _abstractedFileSystem.Directory.Delete(directory);
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

        public string GetPathRoot(string directory)
        {
            var f = new FileInfo(directory);
            return _abstractedFileSystem.Path.GetPathRoot(f.FullName);
        }

        public Result<FileSystemResult> LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes)
        {
            Log.Debug("Looking up path: {query}");

            // If path is invalid return root file system
            if (string.IsNullOrWhiteSpace(query) || !_abstractedFileSystem.Directory.Exists(query))
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

            var lastSeparatorIndex = query.LastIndexOf(_abstractedFileSystem.Path.DirectorySeparatorChar);
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
                var fullPath = _abstractedFileSystem.Path.Combine(directory, fileName);
                if (_abstractedFileSystem.Directory.Exists(fullPath))
                {
                    Log.Warning($"Path: {fullPath} already exists, will overwrite now");
                }

                _abstractedFileSystem.Directory.CreateDirectory(directory);

                var availableSpace = CheckDirectoryAvailableSpace(directory);

                if (availableSpace < fileSize)
                {
                    return Result.Fail(
                        $"There is not enough space available in root directory {directory}");
                }

                Stream fileStream = _abstractedFileSystem.File.Create(fullPath, 4096, FileOptions.Asynchronous);

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
            return _abstractedFileSystem.Path.GetFullPath(_abstractedFileSystem.Path.Combine(_pathProvider.RootDirectory, relativePath));
        }

        #endregion

        #region Private Methods

        private Result CreateConfigDirectory()
        {
            try
            {
                if (!_abstractedFileSystem.Directory.Exists(_pathProvider.ConfigDirectory))
                {
                    Log.Debug("Config directory doesn't exist, will create now.");

                    _abstractedFileSystem.Directory.CreateDirectory(_pathProvider.ConfigDirectory);

                    Log.Debug($"Directory: \"{_pathProvider.ConfigDirectory}\" created!");
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