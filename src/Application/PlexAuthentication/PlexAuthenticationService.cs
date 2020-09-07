using FluentResults;
using MediatR;
using PlexRipper.Application.PlexAuthentication.Queries;
using PlexRipper.Domain;
using System;
using System.Threading.Tasks;
using PlexRipper.Application.Common;

namespace PlexRipper.Application.PlexAuthentication
{
    public class PlexAuthenticationService : IPlexAuthenticationService
    {
        private readonly IMediator _mediator;
        private readonly IPlexApiService _plexApiService;

        public PlexAuthenticationService(IMediator mediator, IPlexApiService plexApiService)
        {
            _mediator = mediator;
            _plexApiService = plexApiService;
        }

        public async Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            if (plexAccount == null)
            {
                Log.Warning("The plexAccount was null");
                return string.Empty;
            }

            if (plexAccount.AuthenticationToken != string.Empty)
            {
                // TODO Make the token refresh limit configurable
                if ((plexAccount.ValidatedAt - DateTime.Now).TotalDays < 30)
                {
                    Log.Information("Plex AuthToken was still valid, using from local DB.");
                    return plexAccount.AuthenticationToken;
                }

                Log.Information("Plex AuthToken has expired, refreshing Plex AuthToken now.");

                return await _plexApiService.RefreshPlexAuthTokenAsync(plexAccount);
            }

            Log.Error($"PlexAccount with Id: {plexAccount.Id} contained an empty AuthToken!");
            return string.Empty;
        }

        /// <summary>
        /// Returns the authentication token needed to communicate with a <see cref="PlexServer"/>
        /// </summary>
        /// <param name="plexAccountId"></param>
        /// <param name="plexServerId"></param>
        /// <returns></returns>
        public Task<Result<string>> GetPlexServerTokenAsync(int plexAccountId, int plexServerId)
        {
            // TODO if there is no token then is should refresh a token
            return _mediator.Send(new GetPlexServerTokenQuery(plexAccountId, plexServerId));
        }

        public async Task<Result<string>> GetPlexServerTokenWithUrl(int plexAccountId, int plexServerId, string serverUrl)
        {
            if (string.IsNullOrEmpty(serverUrl))
            {
                return ResultExtensions.IsNull(nameof(serverUrl));
            }

            var token = await GetPlexServerTokenAsync(plexAccountId, plexServerId);
            if (token.IsFailed)
            {
                return token;
            }

            // TODO verify that download=1 is not needed.
            return Result.Ok($"{serverUrl}?X-Plex-Token={token.Value}");
        }
    }
}