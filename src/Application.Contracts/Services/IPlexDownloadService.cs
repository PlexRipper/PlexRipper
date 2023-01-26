﻿using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexDownloadService
{
    Task<Result> ClearCompleted(List<int> downloadTaskIds = null);

    Task<Result> StartDownloadTask(int downloadTaskId);

    Task<Result> PauseDownloadTask(int downloadTaskId);

    Task<Result> StopDownloadTask(int downloadTaskId);

    Task<Result> RestartDownloadTask(int downloadTaskId);

    Task<Result> DownloadMedia(List<DownloadMediaDTO> downloadTaskOrders);

    Task<Result<bool>> DeleteDownloadTasksAsync(List<int> downloadTaskIds);

    Task<Result<List<DownloadTask>>> GetDownloadTasksAsync();

    Task<Result<DownloadTask>> GetDownloadTaskDetailAsync(int downloadTaskId, CancellationToken cancellationToken);
}