using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using PlexApi.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public class PlexServerConnectionsService : IPlexServerConnectionsService
{
    #region Fields

    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IPlexApiService _plexApiService;
    private readonly ISignalRService _signalRService;

    #endregion

    #region Constructors

    public PlexServerConnectionsService(IMediator mediator, IMapper mapper, ISignalRService signalRService, IPlexApiService plexApiService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _signalRService = signalRService;
        _plexApiService = plexApiService;
    }

    #endregion

    #region Methods

    #region Public



    public async Task<Result> CheckAllConnectionsOfPlexServersByAccountIdAsync(int plexAccountId)
    {
        var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId, true));
        if (plexAccountResult.IsFailed)
        {
            return plexAccountResult
                .WithError($"Could not retrieve any PlexAccount from database with id {plexAccountId}.")
                .LogError();
        }

        var plexServers = plexAccountResult.Value.PlexServers;

        var serverTasks = plexServers.Select(async plexServer => await CheckAllConnectionsOfPlexServerAsync(plexServer.Id));

        var tasksResult = await Task.WhenAll(serverTasks);
        return Result.OkIf(tasksResult.Any(x => x.IsSuccess),
                $"None of the servers that were checked for {nameof(PlexAccount)} with id {plexAccountId} were connectable")
            .LogIfFailed();
    }

    #endregion

    #endregion
}