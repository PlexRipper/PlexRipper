using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Application.FolderPaths.Queries;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Application.PlexDownloads.Queries;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexDownloads
{
    public class PlexDownloadService : IPlexDownloadService
    {
        private readonly IMediator _mediator;
        private readonly IDownloadManager _downloadManager;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IFileSystem _fileSystem;
        private readonly IPlexApiService _plexApiService;

        public PlexDownloadService(IMediator mediator, IDownloadManager downloadManager, IPlexAuthenticationService plexAuthenticationService, IFileSystem fileSystem, IPlexApiService plexApiService)
        {
            _mediator = mediator;
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
            _fileSystem = fileSystem;
            _plexApiService = plexApiService;
        }

        public Task StartDownloadAsync(DownloadTask downloadTask)
        {
            return _downloadManager.StartDownloadAsync(downloadTask);
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexTokenAsync(plexAccount);
        }
        public async Task<Result<DownloadTask>> GetDownloadRequestAsync(int plexAccountId, PlexMovie plexMovie)
        {
            if (plexMovie == null)
            {
                return Result.Fail("plexMovie parameter is null");
            }

            Log.Debug($"Creating download request for movie {plexMovie.Title}");

            var server = plexMovie.PlexLibrary.PlexServer;
            var token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccountId, server.Id);

            if (token.IsFailed)
            {
                return token.ToResult<DownloadTask>();
            }

            // TODO make this dynamic
            // Get the destination folder
            var folderPath = await _mediator.Send(new GetFolderPathByIdQuery(1));
            if (folderPath.IsFailed)
            {
                return Result.Fail("folderPath was null");
            }

            // Retrieve Metadata for this PlexMovie
            var metaData = await _plexApiService.GetMediaMetaDataAsync(token.Value, server.BaseUrl, plexMovie.RatingKey);

            if (metaData != null)
            {
                return Result.Ok(new DownloadTask
                {
                    PlexServerId = server.Id,
                    FolderPathId = folderPath.Value.Id,
                    FolderPath = folderPath.Value,
                    FileLocationUrl = metaData.ObfuscatedFilePath,
                    Title = plexMovie.Title,
                    Status = DownloadStatus.Initialized,
                    FileName = metaData.FileName,
                    PlexServerAuthToken = token.Value,
                    DownloadDirectory = _fileSystem.RootDirectory + "/Movies"
                });
            }

            return Result.Fail($"Failed to retrieve metadata for plex movie with id: {plexMovie.Id}");
        }

        public async Task<Result<bool>> DownloadMovieAsync(int plexAccountId, int plexMovieId)
        {
            var plexMovie = await _mediator.Send(new GetPlexMovieByIdQuery(plexMovieId));
            if (plexMovie.IsFailed)
            {
                return plexMovie.ToResult<bool>();
            }

            var downloadTask = await GetDownloadRequestAsync(plexAccountId, plexMovie.Value);
            if (downloadTask.IsFailed)
            {
                return downloadTask.ToResult<bool>();
            }

            await StartDownloadAsync(downloadTask.Value);
            return Result.Ok(true);
        }

        #region CRUD
        public Task<Result<List<DownloadTask>>> GetAllDownloadsAsync()
        {
            return _mediator.Send(new GetAllDownloadTasksQuery());
        }

        public Task<Result<bool>> DeleteDownloadsAsync(int downloadTaskId)
        {
            _downloadManager.CancelDownload(downloadTaskId);
            return _mediator.Send(new DeleteDownloadTaskByIdCommand(downloadTaskId));
        }




        #endregion
    }
}
