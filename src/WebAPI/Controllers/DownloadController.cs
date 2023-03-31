using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DownloadController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IDownloadCommands _downloadCommands;
    private readonly IDownloadTaskFactory _downloadTaskFactory;

    public DownloadController(
        ILog log,
        IMediator mediator,
        IDownloadCommands downloadCommands,
        IDownloadTaskFactory downloadTaskFactory,
        IMapper mapper,
        INotificationsService notificationsService) : base(log,
        mapper,
        notificationsService)
    {
        _mediator = mediator;
        _downloadCommands = downloadCommands;
        _downloadTaskFactory = downloadTaskFactory;
    }

    // GET: api/<DownloadController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<ServerDownloadProgressDTO>>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetDownloadTasks()
    {
        var result = await _mediator.Send(new GetAllDownloadTasksQuery());

        if (result.IsFailed)
            return InternalServerError(result.ToResult());

        return ToActionResult<List<DownloadTask>, List<ServerDownloadProgressDTO>>(result);
    }

    /// <summary>
    /// POST: "api/(DownloadController)/clear".
    /// </summary>
    /// <returns>Is successful.</returns>
    [HttpPost("clear")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> ClearCompleted([FromBody] List<int> downloadTaskIds)
    {
        return ToActionResult(await _downloadCommands.ClearCompleted(downloadTaskIds));
    }

    /// <summary>
    /// POST: api/(DownloadController)/download/
    /// </summary>
    /// <param name="downloadMedias"></param>
    /// <returns></returns>
    [HttpPost("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> DownloadMedia([FromBody] List<DownloadMediaDTO> downloadMedias)
    {
        _log.DebugLine("Attempting to add download task orders: ");
        foreach (var downloadMediaDto in downloadMedias)
            _log.Debug("DownloadMediaDTO: {@DownloadMediaDto} ", downloadMediaDto);

        var downloadTasks = await _downloadTaskFactory.GenerateAsync(downloadMedias);
        if (downloadTasks.IsFailed)
            return ToActionResult(downloadTasks.ToResult());

        var result = await _downloadCommands.CreateDownloadTasks(downloadTasks.Value);

        return ToActionResult(result);
    }

    #region BatchCommands

    // GET api/<DownloadController>/start/{id:int}
    [HttpGet("start/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StartCommand(int id)
    {
        return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _downloadCommands.StartDownloadTask(id));
    }

    // GET api/<DownloadController>/pause/{id:int}
    [HttpGet("pause/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> PauseCommand(int id)
    {
        return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _downloadCommands.PauseDownloadTask(id));
    }

    // GET api/<DownloadController>/restart/{id:int}
    [HttpGet("restart/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> RestartCommand(int id)
    {
        return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _downloadCommands.RestartDownloadTask(id));
    }

    // GET: api/(DownloadController)/stop/{id:int}
    [HttpGet("stop/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StopCommand(int id)
    {
        return id <= 0 ? BadRequestInvalidId() : ToActionResult(await _downloadCommands.StopDownloadTasks(id));
    }

    /// <summary>
    /// HttpPost api/(DownloadController)/delete
    /// </summary>
    /// <param name="downloadTaskIds">The list of downloadTasks to delete by id.</param>
    /// <returns>HTTP Response.</returns>
    [HttpPost("delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> DeleteCommand([FromBody] List<int> downloadTaskIds)
    {
        if (!downloadTaskIds.Any())
            return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));

        var result = await _downloadCommands.DeleteDownloadTaskClients(downloadTaskIds);
        ;
        return ToActionResult(result.ToResult());
    }

    // GET: api/(DownloadController)/detail/{id:int}
    [HttpGet("detail/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetDetail(int id, CancellationToken token)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(id, true), token);
        return ToActionResult<DownloadTask, DownloadTaskDTO>(downloadTaskResult);
    }

    [HttpPost("preview")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<DownloadPreviewDTO>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> DownloadPreview([FromBody] List<DownloadMediaDTO> downloadMedias, CancellationToken token)
    {
        var result = await _mediator.Send(new GetDownloadPreviewQuery(downloadMedias), token);
        return ToActionResult<List<DownloadPreviewDTO>, List<DownloadPreviewDTO>>(result);
    }

    #endregion
}