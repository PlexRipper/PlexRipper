using System;
using System.Threading.Tasks;
using FluentResultExtensions.lib;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexAuthentication.Queries;
using PlexRipper.Domain;

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

        public async Task<string> GetPlexApiTokenAsync(PlexAccount plexAccount)
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

        /// <inheritdoc/>
        public Task<Result<string>> GetPlexServerTokenAsync(int plexServerId, int plexAccountId = 0)
        {
            // TODO if there is no token then it should refresh a token
            return _mediator.Send(new GetPlexServerTokenQuery(plexServerId, plexAccountId));
        }

        public async Task<Result<string>> GetPlexServerTokenWithUrl(int plexServerId, string serverUrl, int plexAccountId = 0)
        {
            if (string.IsNullOrEmpty(serverUrl))
            {
                return ResultExtensions.IsNull(nameof(serverUrl));
            }

            var token = await GetPlexServerTokenAsync(plexServerId, plexAccountId);
            if (token.IsFailed)
            {
                return token;
            }

            // TODO verify that download=1 is not needed.
            return Result.Ok($"{serverUrl}?X-Plex-Token={token.Value}");
        }
    }
}