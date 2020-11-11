using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using AutoMapper.Configuration.Annotations;

namespace PlexRipper.Domain
{
    [Table("PlexMovie")]
    public class PlexMovie : PlexMedia, IToDownloadTask
    {
        #region Properties

        public List<PlexMovieData> PlexMovieDatas { get; set; }

        public List<PlexMovieGenre> PlexMovieGenres { get; set; }

        public List<PlexMovieRole> PlexMovieRoles { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public List<PlexMovieDataPart> GetParts => PlexMovieDatas.SelectMany(x => x.Parts).ToList();

        [NotMapped]
        public override PlexMediaType Type => PlexMediaType.Movie;

        /// <summary>
        /// A <see cref="PlexMovie"/> can have multiple media parts, which is why we return a list.
        /// </summary>
        /// <returns>The <see cref="DownloadTask"/>s created from all parts.</returns>
        public List<DownloadTask> CreateDownloadTasks()
        {
            var baseDownloadTask = CreateBaseDownloadTask();
            var downloadTasks = new List<DownloadTask>();

            foreach (var part in GetParts)
            {
                var downloadTask = baseDownloadTask;
                downloadTask.Title = Title;
                downloadTask.FileLocationUrl = part.ObfuscatedFilePath;
                downloadTask.DataTotal = part.Size;
                downloadTask.FileName = Path.GetFileName(part.File);
                downloadTask.MediaType = Type;

                downloadTasks.Add(downloadTask);
            }

            return downloadTasks;
        }

        #endregion
    }
}