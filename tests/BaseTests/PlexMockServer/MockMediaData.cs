using System.IO;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class MockMediaData
    {
        public string FileName { get; }

        public string DirectoryPath { get; }

        public long ByteSize => new FileInfo(FilePath).Length;

        public string Md5 { get; }

        public string FilePath { get; }

        public string ParentFolderName  { get; }

        public PlexMediaType Type { get; }

        public string RelativeUrl
        {
            get
            {
                return Type switch
                {
                    PlexMediaType.Movie => $"/media/movies/{ParentFolderName}/{FileName}",
                    _ => "/",
                };
            }
        }

        public MockMediaData(PlexMediaType type, string filePath)
        {
            Type = type;
            FilePath = filePath;
            DirectoryPath = Directory.GetParent(filePath).FullName;
            ParentFolderName = Directory.GetParent(filePath).Name;
            FileName = Path.GetFileName(filePath);
            Md5 = DataFormat.CalculateMD5(filePath);
        }
    }
}