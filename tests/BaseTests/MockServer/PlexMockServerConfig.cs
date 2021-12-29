using System;

namespace PlexRipper.BaseTests
{
    public class PlexMockServerConfig
    {
        public PlexMockServerConfig(int downloadFileSize = 0)
        {
            if (downloadFileSize > 0)
            {
                File = FakeData.GetDownloadFile(40);
            }
        }
        public Uri DownloadUri { get; set; }

        public Uri ServerUri { get; set; }

        public byte[] File { get; init; }
    }
}