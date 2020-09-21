using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    [Table("PlexMovie")]
    public class PlexMovie : PlexMedia
    {
        #region Properties

        public List<PlexMovieData> PlexMovieDatas { get; set; }

        public List<PlexMovieGenre> PlexMovieGenres { get; set; }

        public List<PlexMovieRole> PlexMovieRoles { get; set; }

        #endregion
    }
}