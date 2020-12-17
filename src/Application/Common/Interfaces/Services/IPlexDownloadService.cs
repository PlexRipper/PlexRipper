using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexDownloadService
    {
        Task<string> GetPlexTokenAsync(PlexAccount plexAccount);

        Task<Result<List<PlexServer>>> GetDownloadTasksInServerAsync();

        Task<Result<bool>> DeleteDownloadTaskAsync(int downloadTaskId);

        Result<bool> StopDownloadTask(int downloadTaskId);

        Task<Result<bool>> RestartDownloadTask(int downloadTaskId);

        Task<Result<bool>> ClearCompleted();

        Task<Result<bool>> StartDownloadTask(int downloadTaskId);

        Result<bool> PauseDownloadTask(int downloadTaskId);

        Task<Result<bool>> DownloadMediaAsync(int mediaId, PlexMediaType type, int plexAccountId);

        Task<Result<bool>> DeleteDownloadTasksAsync(IEnumerable<int> downloadTaskIds);

        Task<Result<List<DownloadTask>>> GetDownloadTasksAsync();
    }
}