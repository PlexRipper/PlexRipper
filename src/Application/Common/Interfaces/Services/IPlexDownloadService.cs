using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common.DTO.WebApi;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IPlexDownloadService
    {
        Task<Result<List<PlexServer>>> GetDownloadTasksInServerAsync();

        Task<Result> StopDownloadTask(List<int> downloadTaskIds = null);

        Task<Result> RestartDownloadTask(List<int> downloadTaskIds);

        Task<Result> ClearCompleted(List<int> downloadTaskIds = null);

        Task<Result> StartDownloadTask(int downloadTaskId);

        Task<Result> PauseDownloadTask(int downloadTaskId);

        Task<Result> DownloadMediaAsync(List<DownloadMediaDTO> downloadMedias);

        Task<Result> DeleteDownloadTasksAsync(IEnumerable<int> downloadTaskIds);

        Task<Result<List<DownloadTask>>> GetDownloadTasksAsync();
    }
}