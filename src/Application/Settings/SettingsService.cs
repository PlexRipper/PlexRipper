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