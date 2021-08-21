using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly IPlexLibraryService _plexLibraryService;

        private readonly ISchedulerService _schedulerService;

        private readonly ISignalRService _signalRService;

        private readonly IPlexApiService _plexServiceApi;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly List<int> _currentSyncingPlexServers = new();

        public PlexServerService(
            IMapper mapper,
            IMediator mediator,
            IPlexApiService plexServiceApi,
            IPlexAuthenticationService plexAuthenticationService,
            IPlexLibraryService plexLibraryService,
            ISchedulerService schedulerService,
            ISignalRService signalRService)
        {
            _mapper = mapper;
            _mediator = mediator;
            _plexLibraryService = plexLibraryService;
            _schedulerService = schedulerService;
            _signalRService = signalRService;
            _plexServiceApi = plexServiceApi;
            _plexAuthenticationService = plexAuthenticationService;
        }

        /// <summary>
        /// Retrieves the latest <see cref="PlexServer"/> data, and the corresponding <see cref="PlexLibrary"/>,
        /// from the PlexAPI and stores it in the Database.
        /// </summary>
        /// <param name="plexAccount">The <see cref="PlexAccount"/> used to retrieve the accessible <see cref="PlexServer">PlexServers</see>.</param>
        /// <returns>Is successful.</returns>
        public async Task<Result> RetrieveAccessiblePlexServersAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                return Result.Fail("plexAccount was null").LogWarning();
            }

            Log.Debug($"Refreshing Plex servers for PlexAccount: {plexAccount.Id}");

            var token = await _plexAuthenticationService.GetPlexApiTokenAsync(plexAccount);

            if (string.IsNullOrEmpty(token))
            {
                return Result.Fail("Token was empty").LogWarning();
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

        public async Task<Result> SyncPlexServers(bool forceSync = false)
        {
            var plexServersResult = await GetAllPlexServersAsync(false);
            if (plexServersResult.IsFailed)
            {
                return plexServersResult;
            }

            return await SyncPlexServers(plexServersResult.Value.Select(x => x.Id).ToList());
        }

        public async Task<Result> SyncPlexServers(List<int> plexServerIds, bool forceSync = false)
        {
            var results = new List<Result>();

            foreach (var plexServerId in plexServerIds)
            {
                var result = await SyncPlexServer(plexServerId, forceSync);
                if (result.IsFailed)
                {
                    results.Add(result);
                }
            }

            if (results.Any())
            {
                var failedResult = Result.Fail("Some libraries failed to sync");
                results.ForEach(x => { failedResult.AddNestedErrors(x.Errors); });
                return failedResult.LogError();
            }

            return Result.Ok();
        }

        /// <inheritdoc/>
        public async Task<Result> SyncPlexServer(int plexServerId, bool forceSync = false)
        {
            if (_currentSyncingPlexServers.Contains(plexServerId))
            {
                return Result.Ok($"PlexServer with id {plexServerId} is already syncing").LogWarning();
            }

            _currentSyncingPlexServers.Add(plexServerId);

            var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, true));
            if (plexServerResult.IsFailed)
            {
                _currentSyncingPlexServers.Remove(plexServerId);
                return plexServerResult;
            }

            var plexServer = plexServerResult.Value;
            var results = new List<Result>();

            var plexLibraries = forceSync
                ? plexServer.PlexLibraries
                : plexServer.PlexLibraries.FindAll(
                    x => x.Outdated
                         && (x.Type is PlexMediaType.Movie or PlexMediaType.TvShow));

            if (!plexLibraries.Any())
            {
                _currentSyncingPlexServers.Remove(plexServerId);
                return Result.Ok().WithReason(new Success($"PlexServer {plexServer.Name} with id {plexServer.Id} has no libraries to sync"))
                    .LogInformation();
            }

            // Send progress on every library update
            var progressList = new List<LibraryProgress>();
            var progress = new Action<LibraryProgress>(libraryProgress =>
            {
                var i = progressList.FindIndex(x => x.Id == libraryProgress.Id);
                if (i != -1)
                {
                    progressList.Add(libraryProgress);
                }
                else
                {
                    progressList[i] = libraryProgress;
                }

                _signalRService.SendServerSyncProgressUpdate(new SyncServerProgress(plexServerId, progressList));
            });

            // Sync movie type libraries first because it is a lot quicker than TvShows.
            foreach (var library in plexLibraries.FindAll(x => x.Type == PlexMediaType.Movie))
            {
                var result = await _plexLibraryService.RefreshLibraryMediaAsync(library.Id, progress);
                if (result.IsFailed)
                {
                    results.Add(result);
                }
            }

            foreach (var library in plexLibraries.FindAll(x => x.Type == PlexMediaType.TvShow))
            {
                var result = await _plexLibraryService.RefreshLibraryMediaAsync(library.Id, progress);
                if (result.IsFailed)
                {
                    results.Add(result);
                }
            }

            if (results.Any())
            {
                var failedResult = Result.Fail($"Some libraries failed to sync in PlexServer: {plexServer.Name}");
                results.ForEach(x => { failedResult.AddNestedErrors(x.Errors); });
                _currentSyncingPlexServers.Remove(plexServerId);
                return failedResult.LogError();
            }

            _currentSyncingPlexServers.Remove(plexServerId);
            return Result.Ok();
        }

        /// <summary>
        /// Inspects the <see cref="PlexServer">PlexServers</see> for connectivity and attempts to fix those which return errors.
        /// When successfully connected, the <see cref="PlexLibrary">PlexLibraries</see> are stored in the database.
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <param name="plexServerIds"></param>
        /// <returns></returns>
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

            // Create inspect tasks for all plexServers
            var tasks = plexServers.Select(async plexServer =>
            {
                // Send server inspect status to front-end
                void SendServerProgress(InspectServerProgress progress)
                {
                    progress.PlexServerId = plexServer.Id;
                    _signalRService.SendServerInspectStatusProgress(progress);
                }

                // The call-back action from the httpClient
                var action = new Action<PlexApiClientProgress>(progress => SendServerProgress(_mapper.Map<InspectServerProgress>(progress)));

                // Start with simple status request
                var serverStatusResult = await CheckPlexServerStatusAsync(plexServer, plexAccountId, false, action);
                if (serverStatusResult.IsFailed)
                {
                    Log.Error($"Failed to retrieve the serverStatus for {plexServer.Name} - {plexServer.ServerUrl}");
                    serverStatusResult.LogError();
                    return;
                }

                // Apply possible fixes and try again
                if (!serverStatusResult.Value.IsSuccessful)
                {
                    var dnsFixMsg = $"Attempting to DNS fix the connection with server {plexServer.Name}";
                    Log.Information(dnsFixMsg);
                    SendServerProgress(new InspectServerProgress
                    {
                        AttemptingApplyDNSFix = true,
                        Message = dnsFixMsg,
                    });

                    plexServer.ServerFixApplyDNSFix = true;
                    serverStatusResult = await CheckPlexServerStatusAsync(plexServer, plexAccountId, false);

                    if (serverStatusResult.Value.IsSuccessful)
                    {
                        // DNS fix worked
                        dnsFixMsg = $"Server DNS Fix worked on {plexServer.Name}, connection successful!";
                        Log.Information(dnsFixMsg);
                        SendServerProgress(new InspectServerProgress
                        {
                            Message = dnsFixMsg,
                            Completed = true,
                            ConnectionSuccessful = true,
                            AttemptingApplyDNSFix = true,
                        });
                    }

                    // DNS fix did not work
                    dnsFixMsg = $"Server DNS Fix did not help with server {plexServer.Name} - {plexServer.ServerUrl}";
                    Log.Warning(dnsFixMsg);
                    SendServerProgress(new InspectServerProgress
                    {
                        AttemptingApplyDNSFix = true,
                        Completed = true,
                        Message = dnsFixMsg,
                    });

                    plexServer.ServerFixApplyDNSFix = false;
                    return;
                }

                await _plexLibraryService.RetrieveAccessibleLibrariesAsync(plexAccountResult.Value, plexServer);
            });

            await Task.WhenAll(tasks);

            var updateResult = await _mediator.Send(new UpdatePlexServersCommand(plexServersResult.Value));
            if (updateResult.IsFailed)
            {
                return updateResult;
            }

            // Sync new plexServers with database
            return await SyncPlexServers(plexServerIds, true);
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

        public async Task<Result<PlexServerStatus>> CheckPlexServerStatusAsync(PlexServer plexServer, int plexAccountId = 0, bool trimEntries = true,
            Action<PlexApiClientProgress> progressAction = null)
        {
            // Get plexServer authToken
            var authToken = await _plexAuthenticationService.GetPlexServerTokenAsync(plexServer.Id, plexAccountId);
            if (authToken.IsFailed)
            {
                return authToken.ToResult();
            }

            // Request status
            var serverStatus = await _plexServiceApi.GetPlexServerStatusAsync(authToken.Value, plexServer.ServerUrl, progressAction);
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
        public async Task<Result<List<PlexServer>>> GetAllPlexServersAsync(bool includeLibraries, int plexAccountId = 0)
        {
            // Retrieve all servers
            return await _mediator.Send(new GetAllPlexServersQuery(includeLibraries, plexAccountId));
        }

        #endregion
    }
}