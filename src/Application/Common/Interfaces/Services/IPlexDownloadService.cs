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

        Task<Result<List<DownloadTask>>> StopDownloadTask(List<int> downloadTaskIds);

        Task<Result> RestartDownloadTask(List<int> downloadTaskIds);

        Task<Result> ClearCompleted(List<int> downloadTaskIds = null);

        Task<Result> StartDownloadTask(List<int> downloadTaskIds);

        Task<Result> PauseDownloadTask(List<int> downloadTaskIds);

        Task<Result> DownloadMediaAsync(List<DownloadMediaDTO> downloadMedias);

        Task<Result> DeleteDownloadTasksAsync(List<int> downloadTaskIds);

        Task<Result<List<DownloadTask>>> GetDownloadTasksAsync();
    }
}