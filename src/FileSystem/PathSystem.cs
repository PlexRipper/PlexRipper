using System.IO.Abstractions;
using FluentResults;
using PlexRipper.Application;

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

        /// <inheritdoc/>
        public Result<string> GetDirectoryName(string filePath)
        {
            try
            {
                return Result.Ok(Path.GetDirectoryName(filePath));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        /// <summary>
        /// Replaces invalid characters from a file or folder name
        /// Source: https://stackoverflow.com/a/13617375/8205497
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string SanitizePath(string name)
        {
            var invalids = Path.GetInvalidFileNameChars();
            name = name.Replace(@"·", "-").Replace(": ", " ");
            return string.Join(" ", name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }
    }
}