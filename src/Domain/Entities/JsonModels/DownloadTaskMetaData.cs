using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class DownloadTaskMetaData
    {
        public int ReleaseYear { get; set; }

        public string MovieTitle { get; set; }

        public string TvShowTitle { get; set; }

        public string TvShowSeasonTitle { get; set; }

        public string TvShowEpisodeTitle { get; set; }

        public List<PlexMediaData> MediaData { get; set; }

    }
}