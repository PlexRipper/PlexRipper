using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;
using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DownloadController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IDownloadUrlGenerator _downloadUrlGenerator;

    public DownloadController(
        ILog log,
        IMediator mediator,
        IDownloadUrlGenerator downloadUrlGenerator,
        IMapper mapper,
        INotificationsService notificationsService) : base(log,
        mapper,
        notificationsService)
    {
        _mediator = mediator;
        _downloadUrlGenerator = downloadUrlGenerator;
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
        var result = await _mediator.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds));
        return ToActionResult(result);
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

        var result = await _mediator.Send(new CreateDownloadTasksCommand(downloadMedias));
        return ToActionResult(result);
    }

    #region BatchCommands

    // GET api/<DownloadController>/start/{id:int}
    [HttpGet("start/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StartCommand(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var startResult = await _mediator.Send(new StartDownloadTaskCommand(id));
        return ToActionResult(startResult);
    }

    // GET api/<DownloadController>/pause/{id:int}
    [HttpGet("pause/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> PauseCommand(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var pauseResult = await _mediator.Send(new PauseDownloadTaskCommand(id));
        return ToActionResult(pauseResult);
    }

    // GET api/<DownloadController>/restart/{id:int}
    [HttpGet("restart/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> RestartCommand(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var restartResult = await _mediator.Send(new RestartDownloadTaskCommand(id));
        return ToActionResult(restartResult);
    }

    // GET: api/(DownloadController)/stop/{id:int}
    [HttpGet("stop/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StopCommand(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var stopResult = await _mediator.Send(new StopDownloadTaskCommand(id));
        return ToActionResult(stopResult);
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

        var deleteResult = await _mediator.Send(new DeleteDownloadTaskCommand(downloadTaskIds));
        return ToActionResult(deleteResult);
    }

    // GET: api/(DownloadController)/detail/{id:int}
    [HttpGet("detail/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<DownloadTaskDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetDetail(int id, CancellationToken token)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(id, true), token);

        if (downloadTaskResult.IsFailed)
            return ToActionResult(downloadTaskResult.ToResult());

        if (!downloadTaskResult.Value.IsDownloadable)
            return ToActionResult<DownloadTask, DownloadTaskDTO>(downloadTaskResult);

        // Add DownloadUrl to DownloadTaskDTO
        var downloadTaskDto = _mapper.Map<DownloadTaskDTO>(downloadTaskResult.Value);

        var downloadUrl = await _downloadUrlGenerator.GetDownloadUrl(downloadTaskResult.Value, token);
        if (downloadUrl.IsFailed)
            return ToActionResult<DownloadTask, DownloadTaskDTO>(downloadTaskResult);

        if (downloadTaskResult.Value.IsDownloadable)
            downloadTaskDto.DownloadUrl = downloadUrl.Value;

        return Ok(Result.Ok(downloadTaskDto));
    }

    [HttpPost("preview")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<DownloadPreviewDTO>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> DownloadPreview([FromBody] List<DownloadMediaDTO> downloadMedias, CancellationToken token)
    {
        var result = await _mediator.Send(new GetDownloadPreviewQuery(downloadMedias), token);
        return ToActionResult<List<DownloadPreview>, List<DownloadPreviewDTO>>(result);
    }

    #endregion
}