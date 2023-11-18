using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

public class PlexAccountController : BaseController
{
    #region Fields

    private readonly IMediator _mediator;
    private readonly IPlexAccountService _plexAccountService;

    #endregion

    #region Constructors

    public PlexAccountController(
        ILog log,
        IMediator mediator,
        IPlexAccountService plexAccountService,
        IMapper mapper,
        INotificationsService notificationsService) : base(log,
        mapper, notificationsService)
    {
        _mediator = mediator;
        _plexAccountService = plexAccountService;
    }

    #endregion

    #region Methods

    #region Public

    // GET: api/<PlexAccountController>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<List<PlexAccountDTO>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetAllAccounts([FromQuery] bool enabledOnly = false)
    {
        var result = await _plexAccountService.GetAllPlexAccountsAsync(enabledOnly);
        if (result.IsFailed)
            return BadRequest(result.ToResult());

        var mapResult = _mapper.Map<List<PlexAccountDTO>>(result.Value);
        if (!mapResult.Any() && enabledOnly)
        {
            var logEvent = _log.WarningLine("Could not find any enabled accounts");
            return NotFound(Result.Fail(logEvent.ToLogString()));
        }

        _log.Debug("Returned {PlexAccountCount} accounts", mapResult.Count);
        return Ok(Result.Ok(mapResult).WithSuccess($"Returned {mapResult.Count} accounts"));
    }

    // GET api/<PlexAccountController>/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetAccount(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        return ToActionResult<PlexAccount, PlexAccountDTO>(await _plexAccountService.GetPlexAccountAsync(id));
    }

    // PUT api/<PlexAccountController>/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> Put(int id, [FromBody] PlexAccountDTO updatedAccount, [FromQuery] bool inspect = false)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var mapResult = _mapper.Map<PlexAccount>(updatedAccount);
        return ToActionResult<PlexAccount, PlexAccountDTO>(await _plexAccountService.UpdatePlexAccountAsync(mapResult, inspect));
    }

    // POST api/<AccountController>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResultDTO<PlexAccountDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> CreateAccount([FromBody] PlexAccountDTO newAccount)
    {
        if (newAccount is null)
            return BadRequest("The new account was null");

        var mapResult = _mapper.Map<PlexAccount>(newAccount);
        var createResult = await _plexAccountService.CreatePlexAccountAsync(mapResult);
        return ToActionResult<PlexAccount, PlexAccountDTO>(createResult);
    }

    // DELETE api/<AccountController>/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var deleteResult = await _plexAccountService.DeletePlexAccountAsync(id);
        return ToActionResult(deleteResult);
    }

    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> Validate([FromBody] PlexAccountDTO account)
    {
        var plexAccount = _mapper.Map<PlexAccount>(account);
        var result = await _plexAccountService.ValidatePlexAccountAsync(plexAccount);
        return ToActionResult<PlexAccount, PlexAccountDTO>(result);
    }

    [HttpGet("check/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<bool>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> CheckUsername(string username)
    {
        try
        {
            var result = await _mediator.Send(new CheckUsernameTaskQuery(username));
            return result.IsFailed ? BadRequest(result.ToResult()) : Ok(result);
        }
        catch (Exception e)
        {
            return InternalServerError(e);
        }
    }

    [HttpGet("refresh/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> RefreshPlexAccount(int id)
    {
        return ToActionResult(await _plexAccountService.RefreshPlexAccount(id));
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
            authPinResult = await _plexAccountService.Get2FAPin(clientId);
        else
            authPinResult = await _plexAccountService.Check2FAPin(authPinId, clientId);

        return ToActionResult<AuthPin, AuthPin>(authPinResult);
    }

    #endregion

    #endregion
}