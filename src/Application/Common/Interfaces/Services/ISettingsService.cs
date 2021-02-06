using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Settings.Models;

namespace PlexRipper.Application.Common
{
    public interface ISettingsService
    {
        Result<SettingsModel> GetSettings();

        Result<SettingsModel> UpdateSettings(SettingsModel settingsModel);
    }
}