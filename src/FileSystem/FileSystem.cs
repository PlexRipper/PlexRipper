using FluentResults;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;
using PlexRipper.Domain.Types.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PlexRipper.FileSystem
{
    public class FileSystem : IFileSystem
    {
        private readonly IDiskProvider _diskProvider;
        public string RootDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        public string ConfigDirectory => $"{RootDirectory}/config";


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
                    return Result.Fail<FileStream>(
                        $"There is not enough space available in root directory {directory}");
                }

                var fileStream = File.Create(fullPath);
                // Pre-allocate the required file size
                fileStream.SetLength(fileSize);
                return Result.Ok();

            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public FileSystem(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public string ToAbsolutePath(string relativePath)
        {
            var x = Path.GetFullPath(Path.Combine(RootDirectory, relativePath));
            return x;
        }

        public static long CheckDirectoryAvailableSpace(string directory)
        {
            var root = GetPathRoot(directory);
            DriveInfo drive = new DriveInfo(root);
            return drive.AvailableFreeSpace;
        }

        public static string GetPathRoot(string directory)
        {
            FileInfo f = new FileInfo(directory);
            return Path.GetPathRoot(f.FullName);
        }

        public FileSystemResult LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                if (OsInfo.IsWindows)
                {
                    var result = new FileSystemResult();
                    result.Directories = GetDrives();

                    return result;
                }

                query = "/";
            }
            if (
                allowFoldersWithoutTrailingSlashes &&
                //query.IsPathValid() &&
                Directory.Exists(query))
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

        private List<FileSystemModel> GetDrives()
        {
            return _diskProvider.GetMounts()
                .Select(d => new FileSystemModel
                {
                    Type = FileSystemEntityType.Drive,
                    Name = _diskProvider.GetVolumeName(d),
                    Path = d.RootDirectory,
                    LastModified = null
                })
                .ToList();
        }
    }
}
