using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class FakeDataConfig
    {
        public int Seed { get; set; }

        public string ServerUrl { get; set; } = "https://test-server.com";

        /// <summary>
        /// If none, a random will be picked
        /// </summary>
        public PlexMediaType LibraryType { get; set; } = PlexMediaType.None;

        #region Include

        public bool IncludeLibraries { get; set; }

        public bool IncludeDownloadTasks { get; set; }

        public bool IncludeMultiPartMovies { get; set; }

        #endregion

        #region Count

        public int MediaCount { get; set; } = 10;

        public int DownloadTasksCount { get; set; } = 10;

        public int LibraryCount { get; set; } = 5;

        public int TvShowSeasonCountMax { get; set; } = 5;

        public int TvShowEpisodeCountMax { get; set; } = 10;

        #endregion

        #region RelationshipIds

        public int PlexServerId { get; set; }

        public int PlexLibraryId { get; set; }

        #endregion
    }
}