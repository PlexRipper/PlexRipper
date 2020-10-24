using System;
using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexTvShowEpisode : PlexMedia
    {
        #region Helpers

        public PlexMediaType Type => PlexMediaType.Episode;

        #endregion

        #region Relationships

        public List<PlexTvShowEpisodeData> EpisodeData { get; set; } = new List<PlexTvShowEpisodeData>();

        public PlexTvShowSeason TvShowSeason { get; set; }

        public int TvShowSeasonId { get; set; }

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

        #endregion
    }
}