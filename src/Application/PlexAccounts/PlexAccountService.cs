using Application.Contracts;
using BackgroundServices.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public class PlexAccountService : IPlexAccountService
{
    #region Fields

    private readonly IInspectServerScheduler _inspectServerScheduler;
    private readonly ILog _log;
    private readonly IMediator _mediator;

    private readonly IPlexApiService _plexApiService;

    #endregion

    #region Constructors

    public PlexAccountService(
        ILog log,
        IMediator mediator,
        IPlexApiService plexApiService,
        IInspectServerScheduler inspectServerScheduler)
    {
        _log = log;
        _mediator = mediator;
        _plexApiService = plexApiService;
        _inspectServerScheduler = inspectServerScheduler;
    }

    #endregion

    #region Methods

    #region Public

    /// <inheritdoc/>
    public async Task<Result> RefreshPlexAccount(int plexAccountId = 0)
    {
        var plexAccountIds = new List<int>();

        if (plexAccountId == 0)
        {
            var enabledAccounts = await _mediator.Send(new GetAllPlexAccountsQuery(true));
            if (enabledAccounts.IsFailed)
                return enabledAccounts.ToResult();

            plexAccountIds.AddRange(enabledAccounts.Value.Select(x => x.Id));
        }
        else
            plexAccountIds.Add(plexAccountId);

        foreach (var id in plexAccountIds)
            await _inspectServerScheduler.QueueRefreshAccessiblePlexServersJob(id);

        return Result.Ok();
    }

    #endregion

    #endregion

    #region Authentication

    private string GeneratePlexAccountClientId()
    {
        return StringExtensions.RandomString(24, true, true);
    }

    /// <summary>
    /// If 2FA is enabled for a PlexAccount, then a Pin needs to be requested and checked every second for having be resolved.
    /// </summary>
    /// <param name="clientId">The account clientId.</param>
    /// <returns></returns>
    public Task<Result<AuthPin>> Get2FAPin(string clientId)
    {
        return _plexApiService.Get2FAPin(clientId);
    }

    public Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId)
    {
        return _plexApiService.Check2FAPin(pinId, clientId);
    }

    #endregion
}