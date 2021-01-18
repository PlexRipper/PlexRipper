using System.IO;
using System.Reflection;

namespace PlexRipper.Domain
{
    public static class FileSystemPaths
    {
        public static string RootDirectory
        {
            get
            {
                switch (OsInfo.CurrentOS)
                {
                    case OperatingSystemPlatform.Linux:
                    case OperatingSystemPlatform.Osx:
                        return "/";
                    case OperatingSystemPlatform.Windows:
                        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                    default:
                        return "/";
                }
            }
        }

        public static string ConfigDirectory => Path.Combine(RootDirectory, "config");

        public static string LogsDirectory => Path.Combine(RootDirectory, "config", "logs");
    }
}