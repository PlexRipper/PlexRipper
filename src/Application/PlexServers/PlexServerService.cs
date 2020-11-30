using System.Collections.Generic;
using System.Linq;
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

        private readonly IPlexApiService _plexServiceApi;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        public PlexServerService(
            IMediator mediator,
            IPlexApiService plexServiceApi,
            IPlexAuthenticationService plexAuthenticationService,
            IPlexLibraryService plexLibraryService)
        {
            _mediator = mediator;
            _plexLibraryService = plexLibraryService;
            _plexServiceApi = plexServiceApi;
            _plexAuthenticationService = plexAuthenticationService;
        }

        /// <summary>
        /// Retrieves the latest <see cref="PlexServer"/> data, and the corresponding <see cref="PlexLibrary"/>, from the PlexAPI and stores it in the Database.
        /// </summary>
        /// <param name="plexAccount">PlexAccount to use to retrieve the servers</param>
        /// <returns>Is successful</returns>
        public async Task<Result<bool>> RefreshPlexServersAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning("plexAccount was null");
                return Result.Fail("plexAccount was null");
            }

            Log.Debug($"Refreshing PlexLibraries for PlexAccount: {plexAccount.Id}");

            var token = await _plexAuthenticationService.GetPlexTokenAsync(plexAccount);

            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("Token was empty");
                return Result.Fail("Token was empty");
            }

            var serverList = await _plexServiceApi.GetServerAsync(token);

            // First add or update the plex servers
            var result = await _mediator.Send(new AddOrUpdatePlexLibrariesCommand(plexAccount, serverList));
            if (result.IsFailed)
            {
                return result.ToResult();
            }

            return Result.Ok(true);
        }

        /// <summary>
        /// Check if the <see cref="PlexServer"/> is available and log the status.
        /// </summary>
        /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to get the latest status for.</param>
        /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to authenticate with.</param>
        /// <returns>The latest <see cref="PlexServerStatus"/>.</returns>
        public async Task<Result<PlexServerStatus>> CheckPlexServerStatusAsync(int plexServerId, int plexAccountId = 0)
        {
            // Get plexServer entity
            var plexServer = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId));
            if (plexServer.IsFailed)
            {
                return plexServer.ToResult();
            }

            // Get plexServer authToken
            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServerId, plexAccountId);
            if (authToken.IsFailed)
            {
                return authToken.ToResult();
            }

            // Request status
            var serverStatus = await _plexServiceApi.GetPlexServerStatusAsync(authToken.Value, plexServer.Value.ServerUrl);
            serverStatus.PlexServer = plexServer.Value;
            serverStatus.PlexServerId = plexServer.Value.Id;

            // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
            var result = await _mediator.Send(new CreatePlexServerStatusCommand(serverStatus));
            if (result.IsFailed)
            {
                return result.ToResult();
            }

            return await _mediator.Send(new GetPlexServerStatusByIdQuery(result.Value));
        }

        #region CRUD

        public Task<Result<PlexServer>> GetServerAsync(int plexServerId)
        {
            return _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
        }

        /// <summary>
        /// Retrieves all <see cref="PlexServer"/>s accessible by this <see cref="PlexAccount"/> from the Database.
        /// </summary>
        /// <param name="plexAccount">The <see cref="PlexAccount"/> to check with</param>
        /// <param name="refresh">Should the <see cref="PlexServer"/>s data be retrieved from the PlexApi</param>
        /// <returns>The list of <see cref="PlexServer"/>s</returns>
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