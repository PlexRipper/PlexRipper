using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexAccounts;
using PlexRipper.Application.Settings.Models;
using PlexRipper.Domain;

namespace PlexRipper.Application.Settings
{
    /// <summary>
    /// Handles API requests concerning the <see cref="IUserSettings"/>.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly IMediator _mediator;

        private readonly IUserSettings _userSettings;

        public SettingsService(IMediator mediator, IUserSettings userSettings)
        {
            _mediator = mediator;
            _userSettings = userSettings;
        }

        public Result<bool> SetActivePlexAccountAsync(int accountId)
        {
            _userSettings.AccountSettings.ActiveAccountId = accountId;
            if (_userSettings.AccountSettings.ActiveAccountId == accountId)
            {
                return Result.Ok(true);
            }

            return Result.Fail(
                $"Could not set value {accountId} to the ActiveAccountId, value remains {_userSettings.AccountSettings.ActiveAccountId}");
        }

        public async Task<Result<PlexAccount>> GetActivePlexAccountAsync()
        {
            int id = _userSettings.AccountSettings.ActiveAccountId;

            if (id == 0)
            {
                Log.Warning("The ActivePlexAccountId was the default value of 0, will set to the first plex account found in the database.");
                var result = await _mediator.Send(new GetAllPlexAccountsQuery(true, true));
                if (result.IsFailed)
                {
                    return result.ToResult();
                }

                if (result.Value.Count > 0)
                {
                    _userSettings.AccountSettings.ActiveAccountId = result.Value[0].Id;
                    return Result.Ok(result.Value[0]);
                }

                string msg = "There are no plexAccounts available to set as the active one";
                Log.Error(msg);
                return Result.Fail(msg);
            }
            else
            {
                // Retrieve with PlexLibraries
                var result = await _mediator.Send(new GetPlexAccountByIdQuery(id, true, true));
                if (result.IsFailed)
                {
                    return result;
                }

                return Result.Ok(result.Value);
            }
        }

        public Result<SettingsModel> GetSettings()
        {
            return Result.Ok(_userSettings as SettingsModel);
        }

        public async Task<Result<bool>> UpdateSettings(SettingsModel settingsModel)
        {
            _userSettings.UpdateSettings(settingsModel);
            return Result.Ok(true);
        }
    }
}