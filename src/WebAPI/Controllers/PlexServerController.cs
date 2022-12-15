using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlexServerController : BaseController
{
    private readonly IPlexServerService _plexServerService;

    public PlexServerController(IPlexServerService plexServerService, IMapper mapper, INotificationsService notificationsService) : base(mapper, notificationsService)
    {
        _plexServerService = plexServerService;
    }

    // GET api/<PlexServerController>/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerDTO>>))]
    public async Task<IActionResult> GetAll()
    {
        return ToActionResult<List<PlexServer>, List<PlexServerDTO>>(await _plexServerService.GetAllPlexServersAsync(false));
    }

    // GET api/<PlexServerController>/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        return ToActionResult<PlexServer, PlexServerDTO>(await _plexServerService.GetServerAsync(id));
    }

    // GET api/<PlexServerController>/5/inspect
    [HttpGet("{id:int}/inspect")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> InspectServer(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        return ToActionResult<PlexServer, PlexServerDTO>(await _plexServerService.InspectPlexServer(id));
    }

    // GET api/<PlexServerController>/5/sync
    [HttpGet("{plexServerId:int}/sync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> SyncServer(int plexServerId, [FromQuery] bool forceSync = false)
    {
        if (plexServerId <= 0)
            return BadRequestInvalidId(nameof(plexServerId));

        return ToActionResult(await _plexServerService.SyncPlexServer(plexServerId, forceSync));
    }

    // PUT api/<PlexServerController>/5/sync
    [HttpPut("{plexServerId:int}/preferred-connection/{plexServerConnectionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> SetPreferredConnection(int plexServerId, int plexServerConnectionId)
    {
        if (plexServerId <= 0)
            return BadRequestInvalidId(nameof(plexServerId));

        if (plexServerConnectionId <= 0)
            return BadRequestInvalidId(nameof(plexServerConnectionId));

        return ToActionResult(await _plexServerService.SetPreferredConnection(plexServerId, plexServerConnectionId));
    }
}