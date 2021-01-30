using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
{
    [Table("PlexMovie")]
    public class PlexMovie : PlexMedia, IToDownloadTask
    {
        #region Relationships

        public List<PlexMovieGenre> PlexMovieGenres { get; set; }

        public List<PlexMovieRole> PlexMovieRoles { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public List<PlexMediaDataPart> MovieParts => MovieData.SelectMany(x => x.Parts).ToList();

        [NotMapped]
        public List<PlexMediaData> MovieData => MediaData.MediaData;

        [NotMapped]
        public override PlexMediaType Type => PlexMediaType.Movie;

        /// <summary>
        /// A <see cref="PlexMovie"/> can have multiple media parts, which is why we return a list.
        /// </summary>
        /// <returns>The <see cref="DownloadTask"/>s created from all parts.</returns>
        public List<DownloadTask> CreateDownloadTasks()
        {
            var downloadTask = CreateBaseDownloadTask();
            downloadTask.MediaType = Type;
            downloadTask.MetaData.MovieTitle = Title;
            downloadTask.MetaData.MediaData = MovieData;
            downloadTask.MetaData.MovieKey = Key;

            return new List<DownloadTask>
            {
                downloadTask,
            };
        }

        #endregion
    }
}