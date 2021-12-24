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

        private readonly IDiskSystem _diskSystem;

        #endregion

        #region Constructor

        public FileSystem(IPathProvider pathProvider, System.IO.Abstractions.IFileSystem abstractedFileSystem, IDiskProvider diskProvider,
            IDiskSystem diskSystem)
        {
            _pathProvider = pathProvider;
            _abstractedFileSystem = abstractedFileSystem;
            _diskProvider = diskProvider;
            _diskSystem = diskSystem;
        }

        #endregion

        #region Public Methods

        public Result CreateDirectory(string directory)
        {
            try
            {
                return Result.Ok(_abstractedFileSystem.Directory.CreateDirectory(directory));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        public bool FileExists(string path)
        {
            return !string.IsNullOrEmpty(path) && _abstractedFileSystem.File.Exists(path);
        }

        public Result<Stream> Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            try
            {
                return Result.Ok(_abstractedFileSystem.File.Open(path, mode, access, share));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        public Result<Stream> Create(string path, int bufferSize, FileOptions options)
        {
            try
            {
                return Result.Ok(_abstractedFileSystem.File.Create(path, bufferSize, options));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
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

                var availableSpace = _diskSystem.HasDirectoryEnoughAvailableSpace(directory, fileSize);
                if (availableSpace.IsFailed)
                {
                    return availableSpace.LogError();
                }

                var createResult = Create(fullPath, 4096, FileOptions.Asynchronous);
                if (createResult.IsFailed)
                {
                    return createResult.ToResult().LogError();
                }

                Stream fileStream = createResult.Value;

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