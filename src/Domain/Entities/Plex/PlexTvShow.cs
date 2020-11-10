using System.Collections.Generic;
using System.Linq;

namespace PlexRipper.Domain
{
    public class PlexTvShow : PlexMedia, IToDownloadTask
    {
        public PlexMediaType Type => PlexMediaType.TvShow;

        #region Relationships

        public List<PlexTvShowGenre> PlexTvShowGenres { get; set; }

        public List<PlexTvShowRole> PlexTvShowRoles { get; set; }

        public List<PlexTvShowSeason> Seasons { get; set; }

        #endregion

        #region Helpers

        public List<DownloadTask> CreateDownloadTasks()
        {
            var downloadTasks = Seasons.SelectMany(x => x.CreateDownloadTasks()).ToList();

            downloadTasks.ForEach(downloadTask => downloadTask.TitleTvShow = Title);

            return downloadTasks;
        }

        #endregion
    }
}