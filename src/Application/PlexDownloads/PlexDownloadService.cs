using Application.Contracts;
using Data.Contracts;

namespace PlexRipper.Application;

public class PlexDownloadService : IPlexDownloadService
{
    #region Fields

    private readonly IDownloadTaskFactory _downloadTaskFactory;

    private readonly IDownloadCommands _downloadCommands;

    private readonly IMediator _mediator;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PlexDownloadService"/> class.
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="downloadTaskFactory"></param>
    /// <param name="downloadCommands"></param>
    public PlexDownloadService(
        IMediator mediator,
        IDownloadTaskFactory downloadTaskFactory,
        IDownloadCommands downloadCommands)
    {
        _mediator = mediator;
        _downloadTaskFactory = downloadTaskFactory;
        _downloadCommands = downloadCommands;
    }

    #endregion

    #region Methods

    #region Public

    public async Task<Result<List<DownloadTask>>> GetDownloadTasksAsync()
    {
        return await _mediator.Send(new GetAllDownloadTasksQuery());
    }

    public async Task<Result<DownloadTask>> GetDownloadTaskDetailAsync(int downloadTaskId, CancellationToken cancellationToken)
    {
        return await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true), cancellationToken);
    }

    #region Commands

    public async Task<Result> DownloadMedia(List<DownloadMediaDTO> downloadTaskOrders)
    {
        Log.Debug($"Attempting to add download task orders: ");
        foreach (var downloadMediaDto in downloadTaskOrders)
            Log.Debug($"{downloadMediaDto} ");

        var downloadTasks = await _downloadTaskFactory.GenerateAsync(downloadTaskOrders);
        if (downloadTasks.IsFailed)
            return downloadTasks.ToResult();

        return await _downloadCommands.CreateDownloadTasks(downloadTasks.Value);
    }

    public async Task<Result<bool>> DeleteDownloadTasksAsync(List<int> downloadTaskIds)
    {
        return await _downloadCommands.DeleteDownloadTaskClients(downloadTaskIds);
    }

    public Task<Result> RestartDownloadTask(int downloadTaskId)
    {
        return _downloadCommands.RestartDownloadTask(downloadTaskId);
    }

    public async Task<Result> StopDownloadTask(int downloadTaskId)
    {
        var result = await _downloadCommands.StopDownloadTasks(downloadTaskId);
        return result.IsSuccess ? Result.Ok() : result.ToResult();
    }

    public Task<Result> StartDownloadTask(int downloadTaskId)
    {
        return _downloadCommands.ResumeDownloadTask(downloadTaskId);
    }

    public Task<Result> PauseDownloadTask(int downloadTaskId)
    {
        return _downloadCommands.PauseDownload(downloadTaskId);
    }

    public Task<Result> ClearCompleted(List<int> downloadTaskIds)
    {
        return _downloadCommands.ClearCompleted(downloadTaskIds);
    }

    #endregion

    #endregion

    #endregion
}