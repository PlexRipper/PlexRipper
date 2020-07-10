using FluentResults;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;
using System;
using System.IO;
using System.Reflection;

namespace PlexRipper.FileSystem
{
    public class FileSystem : IFileSystem
    {
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
    }
}
