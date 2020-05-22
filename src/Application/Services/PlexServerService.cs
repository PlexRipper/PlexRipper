using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Repositories;
using PlexRipper.Domain.Entities;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexServerService : IPlexServerService
    {
        private readonly IPlexServerRepository _plexServerRepository;
        private readonly IPlexApiService _plexServiceApi;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private ILogger Log { get; }

        public PlexServerService(IPlexServerRepository plexServerRepository, IPlexApiService plexServiceApi, IPlexAuthenticationService plexAuthenticationService, ILogger logger)
        {
            _plexServerRepository = plexServerRepository;
            _plexServiceApi = plexServiceApi;
            _plexAuthenticationService = plexAuthenticationService;
            Log = logger;
        }


        public async Task<List<PlexServer>> GetServers(PlexAccount plexAccount, bool refresh = false)
        {
            if (plexAccount == null)
            {
                Log.Warning($"{nameof(GetServers)} => The plexAccount was null");
                return new List<PlexServer>();
            }

            // Retrieve all servers
            var serverList = await _plexServerRepository.GetServers(plexAccount.Id);
            if (refresh || !serverList.Any())
            {
                if (!serverList.Any())
                {
                    Log.Warning($"{nameof(GetServers)} => PlexAccount {plexAccount.Id} did not have any PlexServers assigned. Forcing {nameof(RefreshServersAsync)} was null");
                }

                var refreshSuccess = await RefreshServersAsync(plexAccount);
                if (refreshSuccess)
                {
                    serverList = await _plexServerRepository.GetServers(plexAccount.Id);
                }
            }

            return serverList.ToList();
        }

        public async Task<List<PlexServer>> AddServers(PlexAccount plexAccount, List<PlexServer> servers)
        {

            //foreach (var plexServer in servers)
            //{
            //    plexServer.PlexAccountId = plexAccount.PlexId;
            //}

            await _plexServerRepository.AddRangeAsync(servers);
            var x = await _plexServerRepository.GetServers(plexAccount.Id);
            return x.ToList();
        }

        public async Task AddOrUpdatePlexServersAsync(PlexAccount plexAccount, List<PlexServer> servers)
        {

            if (plexAccount == null)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexServersAsync)} => plexAccount was null");
                return;
            }

            if (servers.Count == 0)
            {
                Log.Warning($"{nameof(AddOrUpdatePlexServersAsync)} => servers list was empty");
                return;
            }

            Log.Debug($"{ nameof(AddOrUpdatePlexServersAsync)} => Starting adding or updating servers");
            // Add or update the plex servers
            foreach (var plexServer in servers)
            {
                // TODO Might need a better way to identify servers, multiple Plex instances might run on the same server.
                var plexServerDB = await
                    _plexServerRepository.FindAsync(x => x.MachineIdentifier == plexServer.MachineIdentifier);

                if (plexServerDB != null)
                {
                    await _plexServerRepository.UpdateAsync(plexServer);
                }
                else
                {
                    // Create entry in many-to-many table
                    var plexAccountServer = new PlexAccountServer
                    {
                        PlexAccountId = plexAccount.Id,
                        PlexServerId = plexServer.Id
                    };
                    plexServer.PlexAccountServers = new List<PlexAccountServer>();
                    plexServer.PlexAccountServers.Add(plexAccountServer);
                    await _plexServerRepository.AddAsync(plexServer);
                }
            }

            // TODO Add check if it was successful


        }

        /// <summary>
        /// Retrieves the latest <see cref="PlexServer"/> from the PlexAPI and stores them in the Database.
        /// </summary>
        /// <param name="plexAccount">PlexAccount to use to retrieve the servers</param>
        /// <returns>Is successful</returns>
        public async Task<bool> RefreshServersAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning($"{nameof(RefreshServersAsync)} => plexAccount was null");
                return false;
            }

            Log.Debug($"{nameof(RefreshServersAsync)} => Refreshing PlexServers for PlexAccount: {plexAccount.Id} was null");
            var token = await _plexAuthenticationService.GetPlexToken(plexAccount);

            if (string.IsNullOrEmpty(token))
            {
                Log.Warning($"{nameof(RefreshServersAsync)} => Token was empty");
                return false;
            }

            var serverList = await _plexServiceApi.GetServerAsync(token);
            await AddOrUpdatePlexServersAsync(plexAccount, serverList);

            return true;
        }
    }
}
