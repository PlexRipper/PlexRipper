using FluentResults;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexDownloadService
    {
        Task<string> GetPlexTokenAsync(PlexAccount plexAccount);
        Task StartDownloadAsync(DownloadTask downloadTask);
        Task<Result<bool>> DownloadMovieAsync(int plexAccountId, int plexMovieId);
        Task<Result<DownloadTask>> GetDownloadRequestAsync(int plexAccountId, PlexMovie plexMovie);
        Task<Result<List<DownloadTask>>> GetAllDownloadsAsync();
    }
}
