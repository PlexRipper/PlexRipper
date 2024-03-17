using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DownloadController : BaseController
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public DownloadController(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IMapper mapper,
        INotificationsService notificationsService) : base(log,
        mapper,
        notificationsService)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    // /// <summary>
    // /// POST: "api/(DownloadController)/clear".
    // /// </summary>
    // /// <returns>Is successful.</returns>
    // [HttpPost("clear")]
    // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    // [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    // public async Task<IActionResult> ClearCompleted([FromBody] List<Guid> downloadTaskIds)
    // {
    //     var result = await _mediator.Send(new ClearCompletedDownloadTasksCommand(downloadTaskIds));
    //     return ToActionResult(result);
    // }

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

    // GET api/<DownloadController>/start/{guid:guid}
    [HttpGet("start/{guid:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StartCommand(Guid guid)
    {
        if (guid == Guid.Empty)
            return BadRequestInvalidId();

        var startResult = await _mediator.Send(new StartDownloadTaskCommand(guid));
        return ToActionResult(startResult);
    }

    // GET api/<DownloadController>/pause/{guid:guid}
    [HttpGet("pause/{guid:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> PauseCommand(Guid guid)
    {
        if (guid == Guid.Empty)
            return BadRequestInvalidId();

        var pauseResult = await _mediator.Send(new PauseDownloadTaskCommand(guid));
        return ToActionResult(pauseResult);
    }

    // GET api/<DownloadController>/restart/{guid:guid}
    [HttpGet("restart/{guid:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> RestartCommand(Guid guid)
    {
        if (guid == Guid.Empty)
            return BadRequestInvalidId();

        var restartResult = await _mediator.Send(new RestartDownloadTaskCommand(guid));
        return ToActionResult(restartResult);
    }

    // GET: api/(DownloadController)/stop/{guid:guid}
    [HttpGet("stop/{guid:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StopCommand(Guid guid)
    {
        if (guid == Guid.Empty)
            return BadRequestInvalidId();

        var stopResult = await _mediator.Send(new StopDownloadTaskCommand(guid));
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
    public async Task<IActionResult> DeleteCommand([FromBody] List<Guid> downloadTaskIds)
    {
        if (!downloadTaskIds.Any())
            return BadRequest(Result.Fail("No list of download task Id's was given in the request body"));

        var deleteResult = await _mediator.Send(new DeleteDownloadTaskCommand(downloadTaskIds));
        return ToActionResult(deleteResult);
    }

    // GET: api/(DownloadController)/detail/{id:guid}/
    [HttpGet("detail/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<DownloadTaskDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken token)
    {
        if (id == Guid.Empty)
            return BadRequestInvalidId();

        var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(id), token);

        if (downloadTaskResult.IsFailed)
            return ToActionResult(downloadTaskResult.ToResult());

        if (!downloadTaskResult.Value.IsDownloadable)
            return ToActionResult<DownloadTaskGeneric, DownloadTaskDTO>(downloadTaskResult);

        // Add DownloadUrl to DownloadTaskDTO
        var downloadTaskDto = _mapper.Map<DownloadTaskDTO>(downloadTaskResult.Value);

        var downloadUrl =
            await _dbContext.GetDownloadUrl(downloadTaskDto.PlexServerId, downloadTaskDto.FileLocationUrl, token);
        if (downloadUrl.IsFailed)
            return ToActionResult<DownloadTaskGeneric, DownloadTaskDTO>(downloadTaskResult);

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