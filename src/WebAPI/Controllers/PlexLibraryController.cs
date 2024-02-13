﻿using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlexLibraryController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IPlexLibraryService _plexLibraryService;

    public PlexLibraryController(
        ILog log,
        IMediator mediator,
        IPlexLibraryService plexLibraryService,
        IMapper mapper,
        INotificationsService notificationsService) : base(log,
        mapper, notificationsService)
    {
        _mediator = mediator;
        _plexLibraryService = plexLibraryService;
    }

    // GET api/<PlexLibrary>/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexLibraryDTO>>))]
    public async Task<IActionResult> GetPlexLibraries()
    {
        var result = await _mediator.Send(new GetAllPlexLibrariesQuery());
        return ToActionResult<List<PlexLibrary>, List<PlexLibraryDTO>>(result);
    }

    // GET api/<PlexLibrary>/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexLibraryDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetPlexLibrary(int id)
    {
        if (id <= 0)
            return BadRequest(id, nameof(id));

        return ToActionResult<PlexLibrary, PlexLibraryDTO>(await _plexLibraryService.GetPlexLibraryAsync(id));
    }

    // POST api/<PlexLibrary>/refresh
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexLibraryDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> RefreshLibrary([FromBody] RefreshPlexLibraryDTO refreshPlexLibraryDto)
    {
        if (refreshPlexLibraryDto is null)
            return BadRequest();

        return ToActionResult<PlexLibrary, PlexLibraryDTO>(await _plexLibraryService.RefreshLibraryMediaAsync(refreshPlexLibraryDto.PlexLibraryId));
    }

    // POST api/<PlexLibrary>/settings/default/destination/{id:int}
    [HttpPut("settings/default/destination")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> UpdateDefaultDestination([FromBody] UpdateDefaultDestinationDTO payload)
    {
        if (payload is null)
            return BadRequest();

        var result = await _mediator.Send(new UpdatePlexLibraryDefaultDestinationByIdCommand(payload.LibraryId, payload.FolderPathId));
        return ToActionResult(result);
    }
}