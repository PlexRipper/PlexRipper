using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexTvShowEpisodeData : PlexMediaData
    {
        #region Properties

        #region Relationships

        public PlexTvShowEpisode PlexTvShowEpisode { get; set; }

        public int PlexTvShowEpisodeId { get; set; }

        public List<PlexTvShowEpisodeDataPart> Parts { get; set; } = new List<PlexTvShowEpisodeDataPart>();

        #endregion

        #endregion

    }
}