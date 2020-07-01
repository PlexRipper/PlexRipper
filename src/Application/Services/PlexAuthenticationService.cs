using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexAuthenticationService : IPlexAuthenticationService
    {
        private readonly IPlexApiService _plexApiService;

        public PlexAuthenticationService(IPlexApiService plexApiService)
        {
            _plexApiService = plexApiService;

        }

        public async Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning($"{nameof(GetPlexTokenAsync)} => The plexAccount was null");
                return string.Empty;
            }

            if (plexAccount.AuthenticationToken != string.Empty)
            {
                // TODO Make the token refresh limit configurable 
                if ((plexAccount.ConfirmedAt - DateTime.Now).TotalDays < 30)
                {
                    Log.Information($"{nameof(GetPlexTokenAsync)} => Plex AuthToken was still valid, using from local DB.");
                    return plexAccount.AuthenticationToken;
                }
                Log.Information($"{nameof(GetPlexTokenAsync)} => Plex AuthToken has expired, refreshing Plex AuthToken now.");

                return await _plexApiService.RefreshPlexAuthTokenAsync(plexAccount);
            }
            Log.Error($"{nameof(GetPlexTokenAsync)} => PlexAccount with Id: {plexAccount.Id} contained an empty AuthToken!");
            return string.Empty;
        }
    }
}
