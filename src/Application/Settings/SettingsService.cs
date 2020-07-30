using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly IMediator _mediator;
        private readonly IUserSettings _userSettings;

        public SettingsService(IMediator mediator, IUserSettings userSettings)
        {
            _mediator = mediator;
            _userSettings = userSettings;
        }

        public async Task<Result<PlexAccount>> SetActivePlexAccountAsync(int accountId)
        {
            var result = await _mediator.Send(new GetPlexAccountByIdWithPlexLibrariesQuery(accountId));
            if (result.IsFailed)
            {
                return result;
            }

            _userSettings.ActiveAccountId = result.Value.Id;

            return result;
        }

        public async Task<Result<PlexAccount>> GetActivePlexAccountAsync()
        {
            int id = _userSettings.ActiveAccountId;

            if (id == 0)
            {
                Log.Warning("The ActivePlexAccountId was the default value of 0, will set to the first plex account found in the database.");
                var result = await _mediator.Send(new GetAllPlexAccountsQuery());
                if (result.IsFailed)
                {
                    return result.ToResult();
                }

                if (result.Value.Count > 0)
                {
                    _userSettings.ActiveAccountId = result.Value[0].Id;
                    return Result.Ok(result.Value[0]);
                }
                string msg = "There are no plexAccounts available to set as the active one";
                Log.Error(msg);
                return Result.Fail(msg);
            }
            else
            {
                var result = await _mediator.Send(new GetPlexAccountByIdWithPlexLibrariesQuery(id));
                if (result.IsFailed)
                {
                    return result;
                }
                return Result.Ok(result.Value);
            }
        }
    }
}
