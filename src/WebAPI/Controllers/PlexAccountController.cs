using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexApi.Contracts;
using PlexRipper.Application;

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

    // GET api/<PlexAccountController>/5
    [HttpGet("{accountId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> GetAccount(int accountId)
    {
        if (accountId <= 0)
            return BadRequestInvalidId();

        var result = await _mediator.Send(new GetPlexAccountByIdQuery(accountId, true, true));

        if (result.Value != null)
            _log.Debug("Found an Account with the id: {AccountId}", accountId);

        _log.Warning("Could not find an Account with id: {AccountId}", accountId);

        return ToActionResult<PlexAccount, PlexAccountDTO>(result);
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
        var result = await _mediator.Send(new UpdatePlexAccountCommand(mapResult, inspect));
        return ToActionResult<PlexAccount, PlexAccountDTO>(result);
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

        var createResult = await _mediator.Send(new CreatePlexAccountCommand(mapResult));
        if (createResult.IsFailed)
            return ToActionResult(createResult.ToResult());

        var getResult = await _mediator.Send(new GetPlexAccountByIdQuery(createResult.Value, true, true));
        return ToActionResult<PlexAccount, PlexAccountDTO>(getResult);
    }

    /// <summary>
    /// Deletes a PlexAccount by its id.
    /// DELETE api/PlexAccount/5
    /// </summary>
    /// <param name="id">The id of the <see cref="PlexAccount"/> to delete.</param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var deleteAccountResult = await _mediator.Send(new DeletePlexAccountCommand(id));
        return ToActionResult(deleteAccountResult);
    }

    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<PlexAccountDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> Validate([FromBody] PlexAccountDTO account)
    {
        var plexAccount = _mapper.Map<PlexAccount>(account);
        var result = await _mediator.Send(new ValidatePlexAccountCommand(plexAccount));
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
            var result = await _mediator.Send(new IsUsernameAvailableQuery(username));
            return result.IsFailed ? BadRequest(result.ToResult()) : Ok(result);
        }
        catch (Exception e)
        {
            return InternalServerError(e);
        }
    }

    // TODO Split up this endpoint between a single and all refresh
    [HttpGet("refresh/{plexAccountId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> RefreshPlexAccount(int plexAccountId)
    {
        var plexAccountIds = new List<int>();

        if (plexAccountId == 0)
        {
            var enabledAccounts = await _mediator.Send(new GetAllPlexAccountsQuery(true));
            if (enabledAccounts.IsFailed)
                return ToActionResult(enabledAccounts.ToResult());

            plexAccountIds.AddRange(enabledAccounts.Value.Select(x => x.Id));
        }
        else
            plexAccountIds.Add(plexAccountId);

        foreach (var id in plexAccountIds)
            await _mediator.Send(new QueueRefreshPlexServerAccessJobCommand(id));

        return ToActionResult(Result.Ok());
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