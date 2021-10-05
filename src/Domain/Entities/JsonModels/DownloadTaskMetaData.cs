using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class DownloadTaskMetaData
    {
        #region Properties

        public int ReleaseYear { get; set; }

        public string MovieTitle { get; set; }

        public string TvShowTitle { get; set; }

        public string TvShowSeasonTitle { get; set; }

        public string TvShowEpisodeTitle { get; set; }

        public int MovieKey { get; set; }

        public int TvShowKey { get; set; }

        public int TvShowSeasonKey { get; set; }

        public int TvShowEpisodeKey { get; set; }

        public List<PlexMediaData> MediaData { get; set; }

        public bool HasMultipleQualities => MediaData.Count > 1;

        #endregion
    }
}