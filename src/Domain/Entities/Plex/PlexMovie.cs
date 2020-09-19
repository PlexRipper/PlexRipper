using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    [Table("PlexMovie")]
    public class PlexMovie : PlexMedia
    {
        #region Properties

        /// <summary>
        /// The primary media file which belongs to this <see cref="PlexMedia"/>.
        /// </summary>
        public List<PlexMovieData> PlexMovieDatas { get; set; }

       // public int PlexMovieData1Id { get; set; }

        /// <summary>
        /// Optional second media file belonging to this <see cref="PlexMedia"/>.
        /// In 90% of cases, a PlexMedia only has 1 media file belonging to it,
        /// in rare cases, a media file might have a second quality version of that media.
        /// Hard-coding two slots for PlexMedia should improve performance when looking up the PlexMediaData,
        /// instead of creating a one-to-many relationship.
        /// </summary>
        //public PlexMovieData PlexMovieData2 { get; set; }

       // public int? PlexMovieData2Id { get; set; }

        public List<PlexMovieGenre> PlexMovieGenres { get; set; }

        public List<PlexMovieRole> PlexMovieRoles { get; set; }

        #endregion
    }
}