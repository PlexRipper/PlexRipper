using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlexRipper.Domain
{
    public class PlexTvShowEpisode : PlexMedia, IToDownloadTask
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

        public List<DownloadTask> CreateDownloadTasks()
        {
            var baseDownloadTask = CreateBaseDownloadTask();
            var downloadTasks = new List<DownloadTask>();
            var parts = EpisodeData.SelectMany(x => x.Parts).ToList();
            foreach (var part in parts)
            {
                var downloadTask = baseDownloadTask;
                downloadTask.Title = Title;
                downloadTask.FileLocationUrl = part.ObfuscatedFilePath;
                downloadTask.DataTotal = part.Size;
                downloadTask.FileName = Path.GetFileName(part.File);
                downloadTask.MediaType = MediaType;
                if (TvShowSeason != null)
                {
                    downloadTask.TitleTvShowSeason = TvShowSeason.Title;
                    if (TvShowSeason.TvShow != null)
                    {
                        downloadTask.TitleTvShow = TvShowSeason.TvShow.Title;
                    }
                }

                downloadTasks.Add(downloadTask);
            }

            return downloadTasks;
        }
    }
}