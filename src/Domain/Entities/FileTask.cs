using System.Collections.Generic;
using System.IO;

namespace PlexRipper.Domain.Entities
{
    public class FileTask
    {
        public List<string> FilePaths { get; set; }
        public string OutputDirectory { get; set; }
        public string FileName { get; set; }
        public string OutputFilePath => Path.Combine(OutputDirectory, Path.GetFileNameWithoutExtension(FileName), FileName);

        public FileTask(string outputDirectory, string fileName, List<string> filePaths)
        {
            OutputDirectory = outputDirectory;
            FileName = fileName;
            FilePaths = filePaths;
        }
    }
}