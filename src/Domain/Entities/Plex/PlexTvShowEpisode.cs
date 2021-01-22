using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    public class PlexTvShowEpisode : PlexMedia, IToDownloadTask
    {
        /// <summary>
        /// The PlexKey of the <see cref="PlexTvShowSeason"/> this belongs too.
        /// </summary>
        public int ParentKey { get; set; }

        #region Relationships

        public PlexTvShowSeason TvShowSeason { get; set; }

        public int TvShowSeasonId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public List<PlexMediaData> EpisodeData => MediaData.MediaData;

        [NotMapped]
        public override PlexMediaType Type => PlexMediaType.Episode;

        #endregion

        public List<DownloadTask> CreateDownloadTasks()
        {
            var downloadTask = CreateBaseDownloadTask();
            downloadTask.MediaType = Type;
            downloadTask.MetaData.TvShowTitle = TvShowSeason?.TvShow?.Title ?? string.Empty;
            downloadTask.MetaData.TvShowSeasonTitle = TvShowSeason?.Title ?? string.Empty;
            downloadTask.MetaData.TvShowEpisodeTitle = Title;
            downloadTask.MetaData.MediaData = EpisodeData;

            return new List<DownloadTask>
            {
                downloadTask,
            };
        }
    }
}