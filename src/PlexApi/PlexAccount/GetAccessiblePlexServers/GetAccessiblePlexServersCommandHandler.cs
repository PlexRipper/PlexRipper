using FastEndpoints;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.PlexApi.GetAccessiblePlexServers;

public class GetAccessiblePlexServersCommandHandler
    : ICommandHandler<GetAccessiblePlexServersCommand, Result<List<PlexServerAccessDTO>>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiClientFactory _plexApiClientFactory;
    private readonly IServerSettingsModule _serverSettingsModule;

    public GetAccessiblePlexServersCommandHandler(
        IPlexRipperDbContext dbContext,
        IPlexApiClientFactory plexApiClientFactory,
        IServerSettingsModule serverSettingsModule
    )
    {
        _dbContext = dbContext;
        _plexApiClientFactory = plexApiClientFactory;
        _serverSettingsModule = serverSettingsModule;
    }

    public async Task<Result<List<PlexServerAccessDTO>>> ExecuteAsync(
        GetAccessiblePlexServersCommand command,
        CancellationToken ct
    )
    {
        var plexAccountId = command.PlexAccountId;
        var plexAccount = await _dbContext.PlexAccounts.GetAsync(plexAccountId, cancellationToken: ct);
        if (plexAccount is null)
        {
            return ResultExtensions.EntityNotFound(nameof(PlexAccount), plexAccountId);
        }

        var plexAccountToken = plexAccount.GetAuthToken;
        if (string.IsNullOrEmpty(plexAccountToken))
            return ResultExtensions.IsEmpty(nameof(plexAccountToken)).LogError();

        var clientId = plexAccount.ClientId;

        var plexDevicesResult = await GetDevices(plexAccountToken, clientId);
        if (plexDevicesResult.IsFailed)
            return plexDevicesResult.ToResult();

        var plexServers = plexDevicesResult
            .Value.FindAll(x => x.Provides.Contains("server"))
            .Select(x => new PlexServerAccessDTO
            {
                AccessToken = new ServerAccessTokenDTO
                {
                    PlexAccountId = plexAccountId,
                    MachineIdentifier = x.ClientIdentifier,
                    AccessToken = x.AccessToken,
                    IsServerOwned = x.Owned,
                },
                PlexServer = new PlexServer
                {
                    Id = 0,
                    Name = x.Name,

                    // The servers have an OwnerId of 0 when it belongs to the PlexAccount that was used to request it.
                    OwnerId = x.OwnerId ?? plexAccount.PlexId,
                    PlexServerOwnerUsername = x.SourceTitle ?? plexAccount.Username,
                    Device = x.Device ?? string.Empty,
                    Platform = x.Platform ?? string.Empty,
                    PlatformVersion = x.PlatformVersion ?? string.Empty,
                    Product = x.Product,
                    ProductVersion = x.ProductVersion,
                    Provides = x.Provides,
                    CreatedAt = x.CreatedAt,
                    LastSeenAt = x.LastSeenAt,
                    MachineIdentifier = x.ClientIdentifier,
                    PublicAddress = x.PublicAddress,
                    PreferredConnectionId = 0,
                    IsEnabled = !_serverSettingsModule.GetIsHidden(x.ClientIdentifier),
                    Home = x.Home,
                    Synced = x.Synced,
                    Relay = x.Relay,
                    Presence = x.Presence,
                    HttpsRequired = x.HttpsRequired,
                    PublicAddressMatches = x.PublicAddressMatches,
                    DnsRebindingProtection = x.DnsRebindingProtection,
                    NatLoopbackSupported = x.NatLoopbackSupported,
                    PlexAccountServers = [],
                    PlexLibraries = [],
                    ServerStatus = [],
                    PlexServerConnections = x
                        .Connections.Select(y => new PlexServerConnection
                        {
                            Id = 0,
                            Protocol = y.Protocol.ToString().ToLower(),
                            Address = y.Address,
                            Port = y.Port,
                            Local = y.Local,
                            Relay = y.Relay,
                            IPv4 = y.Address.IsIpAddress() && !y.IPv6,
                            IPv6 = y.IPv6,
                            Url = y.Uri,
                            PlexServer = null,
                            PlexServerId = 0,
                            LatestConnectionStatus = null,
                            IsCustom = false,
                        })
                        .ToList(),
                },
            })
            .ToList();

        return Result.Ok(plexServers);
    }

    private async Task<Result<List<PlexDevice>>> GetDevices(string plexToken, string clientId)
    {
        var plexTvClient = _plexApiClientFactory.CreateTvClient(
            plexToken,
            new PlexApiClientOptions { ConnectionUrl = string.Empty, Timeout = 15 }
        );
        var result = await Task.WhenAll(
            plexTvClient.Plex.GetServerResourcesAsync(clientID: clientId).ToResponse(),
            plexTvClient
                .Plex.GetServerResourcesAsync(
                    clientID: clientId,
                    includeHttps: IncludeHttps.Enable,
                    includeRelay: IncludeRelay.Enable,
                    includeIPv6: IncludeIPv6.Enable
                )
                .ToResponse()
        );

        if (result[0].IsFailed && result[1].IsFailed)
            return Result.Merge(result[0].ToResult(), result[1].ToResult());

        if (result[0].IsFailed && result[1].IsSuccess)
            return result[1].ToApiResult(x => x.PlexDevices ?? []);

        if (result[0].IsSuccess && result[1].IsFailed)
            return result[0].ToApiResult(x => x.PlexDevices ?? []);

        var deviceList1 = result[0].Value?.PlexDevices?.FindAll(x => x.Provides.Contains("server")) ?? [];
        var deviceList2 = result[1].Value?.PlexDevices?.FindAll(x => x.Provides.Contains("server")) ?? [];

        var uniqueConnections = new HashSet<string>();

        foreach (var device1 in deviceList1)
        {
            var device2 = deviceList2.FirstOrDefault(x => x.ClientIdentifier == device1.ClientIdentifier);
            if (device2 is null || !device2.Connections.Any())
                continue;

            var serverConnections = device1.Connections.Concat(device2.Connections).ToList();

            device1.Connections.Clear();

            foreach (var connection in serverConnections)
            {
                if (uniqueConnections.Add(connection.Uri))
                {
                    device1.Connections.Add(connection);
                }
            }
        }

        return Result.Ok(deviceList1);
    }
}
