using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PlexRipper.Domain
{
    [Table("PlexTvShow")]
    public class PlexTvShow : PlexMedia, IToDownloadTask
    {
        public override PlexMediaType Type => PlexMediaType.TvShow;

        #region Relationships

        public List<PlexTvShowGenre> PlexTvShowGenres { get; set; }

        public List<PlexTvShowRole> PlexTvShowRoles { get; set; }

        public List<PlexTvShowSeason> Seasons { get; set; }

        #endregion

        #region Helpers

        public List<DownloadTask> CreateDownloadTasks()
        {
            var downloadTasks = Seasons.SelectMany(x => x.CreateDownloadTasks()).ToList();

            downloadTasks.ForEach(downloadTask =>
            {
                downloadTask.MetaData.TvShowTitle = Title;
                downloadTask.MetaData.TvShowKey = Key;
            });

            return downloadTasks;
        }

        #endregion
    }
}