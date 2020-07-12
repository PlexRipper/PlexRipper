using System.Collections.Generic;

namespace PlexRipper.Domain.Types.FileSystem
{
    public class FileSystemResult
    {
        public string Parent { get; set; }
        public List<FileSystemModel> Directories { get; set; }
        public List<FileSystemModel> Files { get; set; }

        public FileSystemResult()
        {
            Directories = new List<FileSystemModel>();
            Files = new List<FileSystemModel>();
        }
    }
}
