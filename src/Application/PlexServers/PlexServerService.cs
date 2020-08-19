using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Application.PlexServers.Commands;
using PlexRipper.Application.PlexServers.Queries;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Application.PlexAuthentication.Queries;

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
        /// Check if the <see cref="PlexServer"/> is available and log the status
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <returns></returns>
        public async Task<Result<PlexServerStatus>> GetPlexServerStatusAsync(PlexAccount plexAccount, PlexServer plexServer)
        {
            // Get plexServer authToken
            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccount.Id, plexServer.Id);

            if (authToken.IsFailed)
            {
                return Result.Fail(new Error("Failed to retrieve the server auth token"));
            }

            // Request status
            var serverStatus = await _plexServiceApi.GetPlexServerStatusAsync(authToken.Value, plexServer.BaseUrl);

            serverStatus.PlexServer = plexServer;
            serverStatus.PlexServerId = plexServer.Id;

            // Add plexServer status to DB, the PlexServerStatus table functions as a server log.
            var result = await _mediator.Send(new CreatePlexServerStatusCommand(serverStatus));
            if (result.IsFailed)
            {
                return result.ToResult();
            }

            return await _mediator.Send(new GetPlexServerStatusByIdQuery(result.Value));
        }


        /// <summary>
        /// This will get all <see cref="PlexLibrary"/>s with their media in the parent <see cref="PlexServer"/>
        /// </summary>
        /// <param name="plexAccount"></param>
        /// <param name="plexServer"></param>
        /// <param name="refresh">Force refresh from PlexApi</param>
        /// <returns></returns>
        public async Task<Result<PlexServer>> GetAllLibraryMediaAsync(PlexAccount plexAccount, PlexServer plexServer, bool refresh = false)
        {
            var plexServerDB = await _mediator.Send(new GetPlexServerByIdQuery(plexServer.Id, true));

            if (refresh)
            {
                foreach (var library in plexServerDB.Value.PlexLibraries)
                {
                    await _plexLibraryService.GetLibraryMediaAsync(plexAccount, plexServerDB.Value, library.Key, true);
                }
                return await _mediator.Send(new GetPlexServerByIdQuery(plexServer.Id, true));
            }
            return plexServerDB;
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

        /// <summary>
        /// Retrieves all <see cref="PlexServer"/>s from the Database with the included <see cref="PlexLibrary"/>s.
        /// </summary>
        /// <returns>The list of <see cref="PlexServer"/>s</returns>
        public async Task<Result<List<PlexServer>>> GetServersAsync()
        {
            // Retrieve all servers
            return await _mediator.Send(new GetAllPlexServersQuery(true));
        }

        #endregion
    }
}