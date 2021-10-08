using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common.WebApi;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexDownloadTaskFactory
    {
        Task<Result<List<DownloadTask>>> GenerateAsync(List<DownloadMediaDTO> downloadMedias);

        /// <summary>
        /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
        /// </summary>
        /// <param name="plexMovieIds">The ids of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        Task<Result<List<DownloadTask>>> GenerateMovieDownloadTasksAsync(List<int> plexMovieIds);

        Task<Result<List<DownloadTask>>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks, int plexAccountId = 0);

        List<DownloadWorkerTask> GenerateDownloadWorkerTasks(DownloadTask downloadTask, int parts);

        /// <summary>
        /// Regenerates <see cref="DownloadTask">DownloadTasks</see> while maintaining the Id and priority.
        /// Will also remove old <see cref="DownloadWorkerTask">DownloadWorkerTasks</see> assigned to the old downloadTasks from the database.
        /// </summary>
        /// <param name="downloadTasks">The <see cref="DownloadTask">DownloadTasks</see> to regenerate.</param>
        /// <returns>A list of newly generated <see cref="DownloadTask">DownloadTasks</see></returns>
        Task<Result<List<DownloadTask>>> RegenerateDownloadTask(List<DownloadTask> downloadTasks);

        Result<List<DownloadTask>> CreateDownloadTasks(List<PlexMovie> movies);

        Task<Result<List<DownloadTask>>> GenerateDownloadTvShowTasksAsync(
            List<int> plexTvShowIds,
            List<int> plexTvShowSeasonIds,
            List<int> plexTvShowEpisodeIds);
    }
}