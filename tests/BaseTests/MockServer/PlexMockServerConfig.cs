using System;

namespace PlexRipper.BaseTests
{
    public class PlexMockServerConfig
    {
        public Uri DownloadUri { get; set; }

        public Uri ServerUri { get; set; }

        public int DownloadFileSize { get; init; }

        public long DownloadFileSizeInBytes => DownloadFileSize * 1024;
    }
}