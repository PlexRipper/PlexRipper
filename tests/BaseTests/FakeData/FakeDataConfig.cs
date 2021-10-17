using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class FakeDataConfig
    {
        public int Seed { get; set; }

        public bool IncludeLibraries { get; set; }

        public string ServerUrl { get; set; } = "https://test-server.com";

        public bool IncludeDownloadTasks { get; set; }

        public int MediaCount { get; set; } = 10;

        public int DownloadTasksMaxCount { get; set; } = 10;

        /// <summary>
        /// If none, a random will be picked
        /// </summary>
        public PlexMediaType LibraryType { get; set; } = PlexMediaType.None;
    }
}