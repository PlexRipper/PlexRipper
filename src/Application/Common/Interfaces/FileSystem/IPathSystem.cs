using FluentResults;

namespace PlexRipper.Application
{
    public interface IPathSystem
    {
        Result<string> Combine(params string[] paths);

        string GetPathRoot(string directory);

        /// <summary>
        /// Returns the directory portion of a file path. This method effectively
        /// removes the last segment of the given file path, i.e. it returns a
        /// string consisting of all characters up to but not including the last
        /// backslash ("\") in the file path. The returned value is null if the
        /// specified path is null, empty, or a root (such as "\", "C:", or
        /// "\\server\share").
        /// </summary>
        /// <remarks>
        /// Directory separators are normalized in the returned string.
        /// </remarks>
        Result<string> GetDirectoryName(string filePath);

        string SanitizePath(string name);
    }
}