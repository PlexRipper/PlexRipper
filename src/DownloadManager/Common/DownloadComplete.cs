using System.Collections.Generic;
using System.Linq;
using PlexRipper.Domain.Common;

namespace PlexRipper.DownloadManager.Common
{
    public class DownloadComplete
    {
        public int Id { get; }
        public List<string> FilePaths => DownloadWorkerCompletes.Select(x => x.FilePath).ToList();
        public string DestinationPath { get; set; }
        public string FileName { get; set; }
        public long DataReceived { get; set; }
        public long DataTotal { get; set; }

        public List<DownloadWorkerComplete> DownloadWorkerCompletes { get; set; }

        public DownloadComplete(int id)
        {
            Id = id;
        }
    }
}