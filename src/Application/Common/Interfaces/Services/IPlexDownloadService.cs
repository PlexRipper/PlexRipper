using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IPlexDownloadService
    {
        Task<Result> StopDownloadTask(List<int> downloadTaskIds);

        Task<Result> RestartDownloadTask(List<int> downloadTaskIds);

        Task<Result> ClearCompleted(List<int> downloadTaskIds = null);

        Task<Result> StartDownloadTask(List<int> downloadTaskIds);

        Task<Result> PauseDownloadTask(List<int> downloadTaskIds);

        Task<Result> DownloadMediaAsync(List<DownloadMediaDTO> downloadTaskOrders);

        Task<Result> DeleteDownloadTasksAsync(List<int> downloadTaskIds);

        Task<Result<List<DownloadTask>>> GetDownloadTasksAsync();
    }
}