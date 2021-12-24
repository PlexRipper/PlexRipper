using System;
using System.IO;
using System.IO.Abstractions;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.FileSystem
{
    public class PathSystem : IPathSystem
    {
        private readonly IPath _path;

        public PathSystem(IPath path)
        {
            _path = path;
        }

        public Result<string> Combine(params string[] paths)
        {
            try
            {
                return Result.Ok(_path.Combine(paths));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        public string GetPathRoot(string directory)
        {
            var f = new FileInfo(directory);
            return _path.GetPathRoot(f.FullName);
        }

    }
}