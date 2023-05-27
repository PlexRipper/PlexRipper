﻿using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlexServerConnectionController : BaseController
{
    private readonly IPlexServerConnectionsService _plexServerConnectionsService;

    public PlexServerConnectionController(
        ILog log,
        IMapper mapper,
        IPlexServerConnectionsService plexServerConnectionsService,
        INotificationsService notificationsService) : base(log, mapper, notificationsService)
    {
        _plexServerConnectionsService = plexServerConnectionsService;
    }

    // GET api/<PlexServerConnectionController>/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerConnectionDTO>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetAll()
    {
        var connections = await _plexServerConnectionsService.GetAllPlexServerConnectionsAsync();
        return ToActionResult<List<PlexServerConnection>, List<PlexServerConnectionDTO>>(connections);
    }

    // GET api/<PlexServerConnectionController>/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerConnectionDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        return ToActionResult<PlexServerConnection, PlexServerConnectionDTO>(await _plexServerConnectionsService.GetPlexServerConnectionAsync(id));
    }

    // GET api/<PlexServerConnectionController>/check/5
    [HttpGet("check/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerStatusDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> CheckServerConnection(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        return ToActionResult<PlexServerStatus, PlexServerStatusDTO>(await _plexServerConnectionsService.CheckPlexServerConnectionStatusAsync(id));
    }

    // GET api/<PlexServerController>/check/by-server/5/
    [HttpGet("check/by-server/{plexServerId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerStatusDTO>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> CheckAllServerConnections(int plexServerId)
    {
        if (plexServerId <= 0)
            return BadRequestInvalidId(nameof(plexServerId));

        var result = await _plexServerConnectionsService.CheckAllConnectionsOfPlexServerAsync(plexServerId);

        return ToActionResult<List<PlexServerStatus>, List<PlexServerStatusDTO>>(result);
    }
}