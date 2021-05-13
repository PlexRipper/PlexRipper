using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexDownloads
{
    public class PlexDownloadTaskFactory : IPlexDownloadTaskFactory
    {
        private readonly IMediator _mediator;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly IFolderPathService _folderPathService;

        public PlexDownloadTaskFactory(
            IMediator mediator,
            IPlexAuthenticationService plexAuthenticationService,
            IFolderPathService folderPathService)
        {
            _mediator = mediator;
            _plexAuthenticationService = plexAuthenticationService;
            _folderPathService = folderPathService;
        }

        public async Task<Result<List<DownloadTask>>> GenerateAsync(List<int> mediaIds, PlexMediaType type, int libraryId,
            int plexAccountId = 0)
        {
            var result = await _folderPathService.CheckIfFolderPathsAreValid();
            if (result.IsFailed)
            {
                return result.LogError();
            }

            Result<List<DownloadTask>> downloadTaskResult;

            switch (type)
            {
                case PlexMediaType.Movie:
                    downloadTaskResult = await GenerateMovieDownloadTasksAsync(mediaIds);
                    break;
                case PlexMediaType.TvShow:
                    downloadTaskResult = await GenerateDownloadTvShowTasksAsync(mediaIds);
                    break;
                case PlexMediaType.Season:
                    downloadTaskResult = await GenerateDownloadTvShowSeasonTasksAsync(mediaIds);
                    break;
                case PlexMediaType.Episode:
                    downloadTaskResult = await GenerateDownloadTvShowEpisodeTasksAsync(mediaIds);
                    break;
                case PlexMediaType.Music:
                case PlexMediaType.Album:
                    return Result.Fail("PlexMediaType was Music or Album, this is not yet supported").LogWarning();
                case PlexMediaType.None:
                    return Result.Fail("PlexMediaType was none in DownloadMediaAsync").LogWarning();
                case PlexMediaType.Unknown:
                    return Result.Fail("PlexMediaType was Unknown in DownloadMediaAsync").LogWarning();
                default:
                    return Result.Fail($"PlexMediaType defaulted with value {type.ToString()} in DownloadMediaAsync").LogWarning();
            }

            if (downloadTaskResult.IsFailed)
            {
                return downloadTaskResult.ToResult();
            }

            // Add folder paths
            return await FinalizeDownloadTasks(downloadTaskResult.Value, plexAccountId);
        }

        #region Private Methods

        /// <summary>
        /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
        /// </summary>
        /// <param name="plexMovieIds">The ids of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        public async Task<Result<List<DownloadTask>>> GenerateMovieDownloadTasksAsync(List<int> plexMovieIds)
        {
            Log.Debug($"Creating {plexMovieIds.Count} movie download tasks.");
            var plexMoviesResult = await _mediator.Send(new GetMultiplePlexMoviesByIdsQuery(plexMovieIds, true, true));

            if (plexMoviesResult.IsFailed)
                return plexMoviesResult.ToResult();

            var downloadTasks = new List<DownloadTask>();
            foreach (var plexMovie in plexMoviesResult.Value)
            {
                downloadTasks.AddRange(plexMovie.CreateDownloadTasks());

                Log.Debug($"Created download task(s) for movie: {plexMovie.Title}");
            }

            return Result.Ok(downloadTasks);
        }

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowTasksAsync(List<int> plexTvShowIds)
        {
            Log.Debug($"Creating download tasks for TvShow with id: {plexTvShowIds}");

            var plexTvShows = await _mediator.Send(new GetMultiplePlexTvShowsByIdsWithEpisodesQuery(plexTvShowIds, true, true, true));
            if (plexTvShows.IsFailed) return plexTvShows.ToResult();

            // Parse all contained episodes to DownloadTasks
            var downloadTasks = new List<DownloadTask>();
            foreach (var plexTvShow in plexTvShows.Value)
            {
                var tvShowDownloadTasks = plexTvShow.CreateDownloadTasks();
                foreach (DownloadTask downloadTask in tvShowDownloadTasks)
                {
                    downloadTask.PlexLibrary = plexTvShow.PlexLibrary;
                    downloadTask.PlexServer = plexTvShow.PlexServer;
                }

                downloadTasks.AddRange(tvShowDownloadTasks);
                Log.Debug($"Created download task(s) for tvShow: {plexTvShow.Title}");
            }

            return Result.Ok(downloadTasks);
        }

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowSeasonTasksAsync(List<int> plexTvShowSeasonIds)
        {
            Log.Debug($"Creating download request for TvShow season with id: {plexTvShowSeasonIds}");

            var plexTvShowSeasonResult =
                await _mediator.Send(new GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQuery(plexTvShowSeasonIds, true, true, true));
            if (plexTvShowSeasonResult.IsFailed)
                return plexTvShowSeasonResult.ToResult();

            var downloadTasks = new List<DownloadTask>();
            foreach (var plexTvShowSeason in plexTvShowSeasonResult.Value)
            {
                var seasonDownloadTasks = plexTvShowSeason.CreateDownloadTasks();
                foreach (DownloadTask downloadTask in seasonDownloadTasks)
                {
                    downloadTask.PlexLibrary = plexTvShowSeason.PlexLibrary;
                    downloadTask.PlexServer = plexTvShowSeason.PlexServer;
                }

                downloadTasks.AddRange(seasonDownloadTasks);
                Log.Debug($"Created download task(s) for tvShowSeasons: {plexTvShowSeason.Title}");
            }

            return Result.Ok(downloadTasks);
        }

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowEpisodeTasksAsync(List<int> plexTvShowEpisodeId)
        {
            Log.Debug($"Creating download request for TvShow episode with id: {plexTvShowEpisodeId}");

            var plexTvShowEpisodeResult = await _mediator.Send(new GetMultiplePlexTvShowEpisodesByIdQuery(plexTvShowEpisodeId, true, true, true));
            if (plexTvShowEpisodeResult.IsFailed)
                return plexTvShowEpisodeResult.ToResult();

            var downloadTasks = new List<DownloadTask>();
            foreach (var plexTvShowSeason in plexTvShowEpisodeResult.Value)
            {
                downloadTasks.AddRange(plexTvShowSeason.CreateDownloadTasks());

                Log.Debug($"Created download task(s) for tvShowEpisodes: {plexTvShowSeason.Title}");
            }

            return Result.Ok(downloadTasks);
        }

        public async Task<Result<List<DownloadTask>>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks, int plexAccountId = 0)
        {
            if (!downloadTasks.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

            // Get the download folder
            var downloadFolder = await _folderPathService.GetDownloadFolderAsync();
            if (downloadFolder.IsFailed)
                return downloadFolder.ToResult();

            // Get the destination folder
            var destinationFolder = await _folderPathService.GetDestinationFolderByMediaType(downloadTasks.First().MediaType);
            if (destinationFolder.IsFailed)
                return destinationFolder.ToResult();

            // Get plex server access token
            var serverToken = await _plexAuthenticationService.GetPlexServerTokenAsync(downloadTasks.First().PlexServerId, plexAccountId);
            if (serverToken.IsFailed)
                return serverToken.ToResult();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.DownloadFolderId = downloadFolder.Value.Id;
                downloadTask.DownloadFolder = downloadFolder.Value;
                downloadTask.DestinationFolderId = destinationFolder.Value.Id;
                downloadTask.DestinationFolder = destinationFolder.Value;
                downloadTask.ServerToken = serverToken.Value;
            }

            return Result.Ok(downloadTasks);
        }

        #endregion
    }
}