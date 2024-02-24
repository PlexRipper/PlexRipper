using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlexServerController : BaseController
{
    #region Fields

    private readonly IMediator _mediator;

    #endregion

    #region Constructors

    public PlexServerController(
        ILog log,
        IMapper mapper,
        IMediator mediator,
        INotificationsService notificationsService) : base(log, mapper, notificationsService)
    {
        _mediator = mediator;
    }

    #endregion

    #region Methods

    #region Public

    // GET api/<PlexServerController>/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexServerDTO>>))]
    public async Task<IActionResult> GetAll()
    {
        var plexServersResult = await _mediator.Send(new GetAllPlexServersQuery());

        if (plexServersResult.IsFailed)
            return ToActionResult(plexServersResult.ToResult());

        plexServersResult.WithValue(plexServersResult.Value.SortByOwnedOrder());

        return ToActionResult<List<PlexServer>, List<PlexServerDTO>>(plexServersResult);
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

        var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(id, true));
        return ToActionResult<PlexServer, PlexServerDTO>(plexServerResult);
    }

    // GET api/<PlexServerController>/5/inspect
    [HttpGet("{plexServerId:int}/inspect")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> InspectServer(int plexServerId)
    {
        if (plexServerId <= 0)
            return BadRequestInvalidId();

        var result = await _mediator.Send(new QueueInspectPlexServerJobCommand(plexServerId));
        return ToActionResult<PlexServer, PlexServerDTO>(result);
    }

    // GET api/<PlexServerController>/5/inspect
    [HttpGet("{plexServerId:int}/refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexServerDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    public async Task<IActionResult> RefreshPlexServerConnections(int plexServerId)
    {
        if (plexServerId <= 0)
            return BadRequestInvalidId();

        var refreshResult = await _mediator.Send(new RefreshPlexServerConnectionsCommand(plexServerId));
        return ToActionResult<PlexServer, PlexServerDTO>(refreshResult);
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

        var result = await _mediator.Send(new QueueSyncServerJobCommand(plexServerId, true));
        return ToActionResult(result);
    }

    /// <summary>
    /// Sets the preferred connection for a <see cref="PlexServer"/>
    /// </summary>
    /// <param name="plexServerId"></param>
    /// <param name="plexServerConnectionId"></param>
    /// <returns></returns>
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

        var result = await _mediator.Send(new SetPreferredPlexServerConnectionCommand(plexServerId, plexServerConnectionId));

        return ToActionResult(result);
    }

    #endregion

    #endregion
}