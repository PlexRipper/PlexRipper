using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexTvShowEpisodeDataPart : PlexMediaDataPart
    {
        #region Properties

        #endregion

        #region Relationships

        public PlexTvShowEpisodeData PlexTvShowEpisodeData { get; set; }

        public int PlexTvShowEpisodeDataId { get; set; }

        #endregion
    }
}