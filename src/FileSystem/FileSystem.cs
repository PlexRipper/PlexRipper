using PlexRipper.Application.Common.Interfaces.FileSystem;
using System.IO;
using System.Reflection;

namespace PlexRipper.FileSystem
{
    public class FileSystem : IFileSystem
    {
        public string RootDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

        public string ConfigDirectory => $"{RootDirectory}/config";
    }
}
