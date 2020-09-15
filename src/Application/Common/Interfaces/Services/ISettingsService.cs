using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Settings.Models;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface ISettingsService
    {
        Task<Result<PlexAccount>> SetActivePlexAccountAsync(int accountId);

        Task<Result<PlexAccount>> GetActivePlexAccountAsync();

        Result<SettingsModel> GetSettings();

        Task<Result<bool>> UpdateSettings(SettingsModel settingsModel);
    }
}