using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
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
        /// Retrieves the latest <see cref="PlexServer"/> data, and the corresponding <see cref="PlexLibrary"/>, from the PlexAPI and stores it in the Database.
        /// </summary>
        /// <param name="plexAccount">PlexAccount to use to retrieve the servers.</param>
        /// <returns>Is successful.</returns>
        public async Task<Result<bool>> RefreshPlexServersAsync(PlexAccount plexAccount)
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
                return Result.Ok(false);
            }

            // Add initial entry for the plex servers
            var result = await _mediator.Send(new AddOrUpdatePlexServersCommand(plexAccount, serverList));
            if (result.IsFailed)
            {
                return result;
            }

            var plexServersResult = await InspectPlexServers(plexAccount, serverList);
            if (plexServersResult.IsFailed)
            {
                return plexServersResult.ToResult();
            }

            return await _mediator.Send(new UpdatePlexServersCommand(plexServersResult.Value));
        }

        private async Task<Result<List<PlexServer>>> InspectPlexServers(PlexAccount plexAccount, List<PlexServer> plexServers)
        {
            // The servers have an OwnerId of 0 when it belongs to the PlexAccount that was used to request it.
            plexServers.ForEach(plexServer =>
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

            await _signalRService.SendPlexAccountRefreshUpdate(plexAccount.Id, 0, plexServers.Count);

            int finishedCount = 0;

            async Task SendProgress()
            {
                Interlocked.Increment(ref finishedCount);

                // Send progress update to clients
                await _signalRService.SendPlexAccountRefreshUpdate(plexAccount.Id, finishedCount, plexServers.Count);
            }

            var tasks = plexServers.Select(async plexServer =>
            {
                var serverStatusResult = await CheckPlexServerStatusAsync(plexServer, plexAccount.Id, false);
                if (serverStatusResult.IsFailed)
                {
                    Log.Error($"Failed to retrieve the serverStatus for {plexServer.Name} - {plexServer.ServerUrl}");
                    serverStatusResult.LogError();
                    return;
                }

                // Apply possible fixes and try again
                if (!serverStatusResult.Value.IsSuccessful)
                {
                    Log.Information($"Attempting to DNS fix the connection with server {plexServer.Name}");
                    plexServer.ServerFixApplyDNSFix = true;
                    serverStatusResult = await CheckPlexServerStatusAsync(plexServer, plexAccount.Id, false);
                    if (!serverStatusResult.Value.IsSuccessful)
                    {
                        plexServer.ServerFixApplyDNSFix = false;
                        await SendProgress();
                        Log.Error($"ServerFixApplyDNSFix did not help with server {plexServer.Name} - {plexServer.ServerUrl}");
                        return;
                    }
                    Log.Information($"ServerFixApplyDNSFix worked on {plexServer.Name}, connection successful!");
                }

                await _plexLibraryService.RefreshLibrariesAsync(plexAccount, plexServer);

                await SendProgress();
            });

            await Task.WhenAll(tasks);

            return Result.Ok(plexServers);
        }

        /// <summary>
        /// Check if the <see cref="PlexServer"/> is available and log the status.
        /// </summary>
        /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to get the latest status for.</param>
        /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to authenticate with.</param>
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

        /// <summary>
        /// Retrieves all <see cref="PlexServer"/>s accessible by this <see cref="PlexAccount"/> from the Database.
        /// </summary>
        /// <param name="plexAccount">The <see cref="PlexAccount"/> to check with.</param>
        /// <param name="refresh">Should the <see cref="PlexServer"/>s data be retrieved from the PlexApi.</param>
        /// <returns>The list of <see cref="PlexServer"/>s.</returns>
        public async Task<Result<List<PlexServer>>> GetServersAsync(PlexAccount plexAccount, bool refresh = false)
        {
            if (plexAccount == null)
            {
                Log.Warning("The plexAccount was null");
                return Result.Fail("The plexAccount was null");
            }

            // Retrieve all servers
            var serverList = await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccount.Id));
            if (refresh || !serverList.Value.Any())
            {
                if (!serverList.Value.Any())
                {
                    Log.Warning($"PlexAccount {plexAccount.Id} did not have any PlexServers assigned");
                }

                var refreshSuccess = await RefreshPlexServersAsync(plexAccount);
                if (refreshSuccess.IsFailed)
                {
                    return refreshSuccess.ToResult();
                }

                serverList = await _mediator.Send(new GetAllPlexServersByPlexAccountIdQuery(plexAccount.Id));
                if (serverList.IsFailed)
                {
                    return serverList;
                }
            }

            return serverList;
        }

        /// <inheritdoc/>
        public async Task<Result<List<PlexServer>>> GetServersAsync(int plexAccountId = 0)
        {
            // Retrieve all servers
            return await _mediator.Send(new GetAllPlexServersQuery(plexAccountId));
        }

        #endregion
    }
}