using System.Collections.Generic;
using PlexRipper.Domain.Common;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadComplete
    {
        public int Id { get; }
        public List<string> ChuckPaths { get; set; }
        public string DestinationPath { get; set; }
        public string FileName { get; set; }

        public DownloadComplete(int id)
        {
            Id = id;
        }
    }
}