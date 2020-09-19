using System;

namespace PlexRipper.Domain
{
    public class PlexTvShowEpisode : PlexMedia
    {
        #region Helpers

        public PlexMediaType Type => PlexMediaType.Episode;

        #endregion

        #region Relationships

        public PlexTvShowSeason TvShowSeason { get; set; }

        public int TvShowSeasonId { get; set; }

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

        #endregion
    }
}