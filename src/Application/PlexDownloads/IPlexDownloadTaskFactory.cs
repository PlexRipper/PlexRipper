using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public interface IPlexDownloadTaskFactory
    {
        Task<Result<List<DownloadTask>>> GenerateAsync(List<int> mediaIds, PlexMediaType type, int libraryId,
            int plexAccountId = 0);

        /// <summary>
        /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
        /// </summary>
        /// <param name="plexMovieIds">The ids of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        Task<Result<List<DownloadTask>>> GenerateMovieDownloadTasksAsync(List<int> plexMovieIds);

        Task<Result<List<DownloadTask>>> GenerateDownloadTvShowTasksAsync(List<int> plexTvShowIds);

        Task<Result<List<DownloadTask>>> GenerateDownloadTvShowSeasonTasksAsync(List<int> plexTvShowSeasonIds);

        Task<Result<List<DownloadTask>>> GenerateDownloadTvShowEpisodeTasksAsync(List<int> plexTvShowEpisodeId);

        Task<Result<List<DownloadTask>>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks, int plexAccountId = 0);
    }
}