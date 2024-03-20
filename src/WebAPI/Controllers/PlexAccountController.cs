using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexApi.Contracts;

namespace PlexRipper.WebAPI.Controllers;

public class PlexAccountController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IPlexApiService _plexApiService;

    public PlexAccountController(
        ILog log,
        IMediator mediator,
        IMapper mapper,
        IPlexApiService plexApiService,
        INotificationsService notificationsService
    ) : base(log,
        mapper, notificationsService)
    {
        _mediator = mediator;
        _plexApiService = plexApiService;
    }

    // GET api/<PlexAccountController>/authpin/
    [HttpGet("authpin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<AuthPin>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetAndCheck2FaPin([FromQuery] string clientId, [FromQuery] int authPinId = 0)
    {
        if (string.IsNullOrEmpty(clientId))
            return ToActionResult(Result.Fail("Plex Account Client id was empty").Add400BadRequestError());

        Result<AuthPin> authPinResult;
        if (authPinId == 0)
            authPinResult = await _plexApiService.Get2FAPin(clientId);
        else
            authPinResult = await _plexApiService.Check2FAPin(authPinId, clientId);

        return ToActionResult<AuthPin, AuthPin>(authPinResult);
    }
}