using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexMovieData : PlexMediaData
    {
        #region Properties

        #region Relationships

        public PlexMovie PlexMovie { get; set; }

        public int PlexMovieId { get; set; }

        public List<PlexMovieDataPart> Parts { get; set; }

        #endregion

        #endregion

    }
}