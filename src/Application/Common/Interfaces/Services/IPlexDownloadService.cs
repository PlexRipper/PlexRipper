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

        Task<Result> StopDownloadTask(List<int> downloadTaskIds = null);

        Task<Result<bool>> RestartDownloadTask(int downloadTaskId);

        Task<Result<bool>> ClearCompleted(List<int> downloadTaskIds = null);

        Task<Result<bool>> StartDownloadTask(int downloadTaskId);

        Task<Result> PauseDownloadTask(int downloadTaskId);

        Task<Result> DownloadMediaAsync(int mediaId, PlexMediaType type, int plexAccountId);

        Task<Result<bool>> DeleteDownloadTasksAsync(IEnumerable<int> downloadTaskIds);

        Task<Result<List<DownloadTask>>> GetDownloadTasksAsync();

        Task<Result> DownloadMediaAsync(List<int> mediaIds, PlexMediaType type, int libraryId, int accountId = 0);
    }
}