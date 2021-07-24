using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.WebApi;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexServers
{
    public class PlexServerService : IPlexServerService
    {
        private readonly IMediator _mediator;

        private readonly IPlexLibraryService _plexLibraryService;

        private readonly ISignalRService _signalRService;

        private readonly IPlexApiService _plexServiceApi;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        public PlexServerService(
            IMediator mediator,
            IPlexApiService plexServiceApi,
            IPlexAuthenticationService plexAuthenticationService,
            IPlexLibraryService plexLibraryService,
            ISignalRService signalRService)
        {
            _mediator = mediator;
            _plexLibraryService = plexLibraryService;
            _signalRService = signalRService;
            _plexServiceApi = plexServiceApi;
            _plexAuthenticationService = plexAuthenticationService;
        }

        /// <summary>
        /// Retrieves the latest <see cref="PlexServer"/> data, and the corresponding <see cref="PlexLibrary"/>,
        /// from the PlexAPI and stores it in the Database.
        /// </summary>
        /// <param name="plexAccount">PlexAccount to use to retrieve the servers.</param>
        /// <returns>Is successful.</returns>
        public async Task<Result> RefreshPlexServersAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                return Result.Fail("plexAccount was null").LogWarning();
            }

            Log.Debug($"Refreshing Plex servers for PlexAccount: {plexAccount.Id}");

            var token = await _plexAuthenticationService.GetPlexApiTokenAsync(plexAccount);

            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("Token was empty");
                return Result.Fail("Token was empty");
            }

            var serverList = await _plexServiceApi.GetServersAsync(token);

            if (!serverList.Any())
            {
                return Result.Ok();
            }

            // The servers have an OwnerId of 0 when it belongs to the PlexAccount that was used to request it.
            serverList.ForEach(plexServer =>
            {
                if (plexServer.OwnerId == 0)
                {
                    plexServer.OwnerId = plexAccount.PlexId;
                }

                if (plexServer.Port == 443 && plexServer.Scheme == "http")
                {
                    plexServer.Scheme = "https";
                }
            });

            // Add initial entry for the plex servers
            return await _mediator.Send(new AddOrUpdatePlexServersCommand(plexAccount, serverList));
        }

        public async Task<Result> InspectPlexServers(int plexAccountId, List<int> plexServerIds)
        {
            var plexAccountResult = await _mediator.Send(new GetPlexAccountByIdQuery(plexAccountId));
            if (plexAccountResult.IsFailed)
            {
                return plexAccountResult.WithError($"Could not retrieve any PlexAccount from database with id {plexAccountId}.").LogError();
            }

            var plexServersResult = await _mediator.Send(new GetPlexServersByIdsQuery(plexServerIds));
            if (plexServersResult.IsFailed)
            {
                return plexServersResult.WithError("Could not retrieve any PlexServers from database to inspect.").LogError();
            }

            var plexServers = plexServersResult.Value;

            await _signalRService.SendPlexAccountRefreshUpdate(plexAccountId, 0, plexServers.Count);

            int finishedCount = 0;

            async Task SendProgress()
            {
                Interlocked.Increment(ref finishedCount);

                // Send progress update to clients
                await _signalRService.SendPlexAccountRefreshUpdate(plexAccountId, finishedCount, plexServers.Count);
            }

            void SendServerProgress(InspectServerProgress progress)
            {
                // Send progress update to clients
                _signalRService.SendServerInspectStatusProgress(progress);
            }

            var tasks = plexServers.Select(async plexServer =>
            {
                var serverStatusResult = await CheckPlexServerStatusAsync(plexServer, plexAccountId, false);
                if (serverStatusResult.IsFailed)
                {
                    Log.Error($"Failed to retrieve the serverStatus for {plexServer.Name} - {plexServer.ServerUrl}");
                    serverStatusResult.LogError();
                    return;
                }

                SendServerProgress(new InspectServerProgress
                {
                    ConnectionSuccessful = true,
                    PlexServerId = plexServer.Id,
                });

                // Apply possible fixes and try again
                if (!serverStatusResult.Value.IsSuccessful)
                {
                    Log.Information($"Attempting to DNS fix the connection with server {plexServer.Name}");
                    plexServer.ServerFixApplyDNSFix = true;
                    serverStatusResult = await CheckPlexServerStatusAsync(plexServer, plexAccountId, false);

                    if (!serverStatusResult.Value.IsSuccessful)
                    {
                        plexServer.ServerFixApplyDNSFix = false;
                        await SendProgress();
                        Log.Error($"ServerFixApplyDNSFix did not help with server {plexServer.Name} - {plexServer.ServerUrl}");
                        return;
                    }

                    Log.Information($"ServerFixApplyDNSFix worked on {plexServer.Name}, connection successful!");
                }

                await _plexLibraryService.RefreshLibrariesAsync(plexAccountResult.Value, plexServer);

                await SendProgress();
            });

            await Task.WhenAll(tasks);

            return await _mediator.Send(new UpdatePlexServersCommand(plexServersResult.Value));
        }

        /// <summary>
        /// Check if the <see cref="PlexServer"/> is available and log the status.
        /// </summary>
        /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to get the latest status for.</param>
        /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to authenticate with.</param>
        /// <param name="trimEntries">Delete entries which are older than a certain threshold.</param>
        /// <returns>The latest <see cref="PlexServerStatus"/>.</returns>
        public async Task<Result<PlexServerStatus>> CheckPlexServerStatusAsync(int plexServerId, int plexAccountId = 0, bool trimEntries = true)
        {
            // Get plexServer entity
            var plexServer = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId));
            if (plexServer.IsFailed)
            {
                return plexServer.ToResult();
            }

            return await CheckPlexServerStatusAsync(plexServer.Value, plexAccountId, trimEntries);
        }

        public async Task<Result<PlexServerStatus>> CheckPlexServerStatusAsync(PlexServer plexServer, int plexAccountId = 0, bool trimEntries = true)
        {
            // Get plexServer authToken
            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServer.Id, plexAccountId);
            if (authToken.IsFailed)
            {
                return authToken.ToResult();
            }

            // Request status
            var serverStatus = await _plexServiceApi.GetPlexServerStatusAsync(authToken.Value, plexServer.ServerUrl);
            serverStatus.PlexServer = plexServer;
            serverStatus.PlexServerId = plexServer.Id;

            // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
            var result = await _mediator.Send(new CreatePlexServerStatusCommand(serverStatus));
            if (result.IsFailed)
            {
                return result.ToResult();
            }

            if (trimEntries)
            {
                // Ensure that there are not too many PlexServerStatuses stored.
                var trimResult = await _mediator.Send(new TrimPlexServerStatusCommand(plexServer.Id));
                if (trimResult.IsFailed)
                {
                    return trimResult.ToResult();
                }
            }

            return await _mediator.Send(new GetPlexServerStatusByIdQuery(result.Value));
        }

        public Task<Result> RemoveInaccessibleServers()
        {
            return _mediator.Send(new RemoveInaccessibleServersCommand());
        }

        #region CRUD

        public Task<Result<PlexServer>> GetServerAsync(int plexServerId)
        {
            return _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
        }

        /// <inheritdoc/>
        public async Task<Result<List<PlexServer>>> GetAllServersAsync(bool includeLibraries, int plexAccountId = 0)
        {
            // Retrieve all servers
            return await _mediator.Send(new GetAllPlexServersQuery(includeLibraries, plexAccountId));
        }

        #endregion
    }
}