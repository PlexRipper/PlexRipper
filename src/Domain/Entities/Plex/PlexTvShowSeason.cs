using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
{
    public class PlexTvShowSeason : PlexMedia, IToDownloadTask
    {
        /// <summary>
        /// The PlexKey of the tvShow this belongs too.
        /// </summary>
        public int ParentKey { get; set; }

        #region Relationships

        public PlexTvShow TvShow { get; set; }

        public int TvShowId { get; set; }

        public List<PlexTvShowEpisode> Episodes { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public override PlexMediaType Type => PlexMediaType.Season;

        public List<DownloadTask> CreateDownloadTasks()
        {
            var downloadTasks = Episodes.SelectMany(x => x.CreateDownloadTasks()).ToList();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.MetaData.TvShowSeasonTitle = Title;
                downloadTask.MetaData.TvShowSeasonKey = Key;
                downloadTask.MetaData.TvShowKey = ParentKey;
            }

            return downloadTasks;
        }

        #endregion
    }
}