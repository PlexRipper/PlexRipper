using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Settings.Models;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface ISettingsService
    {
        Result<bool> SetActivePlexAccountAsync(int accountId);

        Task<Result<PlexAccount>> GetActivePlexAccountAsync();

        Result<SettingsModel> GetSettings();

        Task<Result<bool>> UpdateSettings(SettingsModel settingsModel);
    }
}