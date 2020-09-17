﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexDownloadService
    {
        Task<string> GetPlexTokenAsync(PlexAccount plexAccount);
        Task<Result<bool>> DownloadMovieAsync(int plexAccountId, int plexMovieId);
        Task<Result<DownloadTask>> GetDownloadTaskAsync(int plexAccountId, PlexMovie plexMovie);
        Task<Result<List<PlexServer>>> GetAllDownloadsAsync();
        Task<Result<bool>> DeleteDownloadTaskAsync(int downloadTaskId);
        Task<Result<bool>> DownloadTvShowAsync(int plexAccountId, int mediaId, PlexMediaType type);
        Task<Result<bool>> StopDownloadTask(int downloadTaskId);
        Task<Result<bool>> RestartDownloadTask(int downloadTaskId);
        Task<Result<bool>> ClearCompleted();
        Task<Result<bool>> StartDownloadTask(int downloadTaskId);
        Result<bool> PauseDownloadTask(int downloadTaskId);
        Task<Result<bool>> DownloadMediaAsync(int plexAccountId, int mediaId, PlexMediaType type);
        Task<Result<bool>> DeleteDownloadTasksAsync(IEnumerable<int> downloadTaskIds);
    }
}
