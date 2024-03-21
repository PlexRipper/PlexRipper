﻿using Application.Contracts;
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

    #endregion
}