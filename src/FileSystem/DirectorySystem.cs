using System;
using System.IO;
using System.Linq;
using FluentResults;
using PlexRipper.Application;

namespace PlexRipper.FileSystem
{
    public class DirectorySystem : IDirectorySystem
    {
        private readonly IPathSystem _pathSystem;

        public DirectorySystem(IPathSystem pathSystem)
        {
            _pathSystem = pathSystem;
        }

        public Result<bool> Exists(string path)
        {
            try
            {
                return Result.Ok(Directory.Exists(path));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        public Result<DirectoryInfo> CreateDirectory(string path)
        {
            try
            {
                return Result.Ok(Directory.CreateDirectory(path));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        public Result CreateDirectoryFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            {
                return Result.Fail("parameter filepath was empty");
            }

            var directoryPathResult = _pathSystem.GetDirectoryName(filePath);
            if (directoryPathResult.IsFailed)
                return directoryPathResult.ToResult();

            if (string.IsNullOrEmpty(directoryPathResult.Value))
            {
                return Result.Fail($"Could not determine the directory name of path: {filePath}");
            }

            return Result.Ok(Directory.CreateDirectory(directoryPathResult.Value)).ToResult();
        }

        /// <inheritdoc />
        public Result DeleteAllFilesFromDirectory(string directory)
        {
            var directoryExistsResult = Exists(directory);
            if (directoryExistsResult.IsFailed)
                return directoryExistsResult.ToResult();

            if (directoryExistsResult.Value)
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

        public Result<string[]> GetFiles(string directoryPath)
        {
            try
            {
                return Result.Ok(Directory.GetFiles(directoryPath));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        public Result Delete(string directoryPath)
        {
            try
            {
                Directory.Delete(directoryPath);
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
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
                var directoryResult = _pathSystem.GetDirectoryName(filePath);
                if (directoryResult.IsFailed)
                    return directoryResult.ToResult();

                var directoryExistsResult = Exists(filePath);
                if (directoryExistsResult.IsFailed)
                    return directoryExistsResult.ToResult();

                var directoryHasFiles = GetFiles(directoryResult.Value);
                if (directoryHasFiles.IsFailed)
                    return directoryHasFiles.ToResult();

                // If the filePath is just an empty directory then delete that.
                if (!string.IsNullOrEmpty(directoryResult.Value) && directoryExistsResult.Value && !directoryHasFiles.Value.Any())
                {
                    Delete(directoryResult.Value);
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
    }
}