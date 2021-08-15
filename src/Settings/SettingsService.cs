using AutoMapper;
using FluentResults;
using PlexRipper.Application.Common;

namespace PlexRipper.Settings
{
    /// <summary>
    /// Handles API requests concerning the <see cref="IUserSettings"/>.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly IMapper _mapper;

        private readonly IUserSettings _userSettings;

        public SettingsService(IMapper mapper, IUserSettings userSettings)
        {
            _mapper = mapper;
            _userSettings = userSettings;
        }

        public Result<ISettingsModel> GetSettings()
        {
            return Result.Ok((ISettingsModel)_userSettings);
        }

        public Result<ISettingsModel> UpdateSettings(ISettingsModel settingsModel)
        {
            var updateResult = _userSettings.UpdateSettings(settingsModel);
            if (updateResult.IsFailed)
            {
                return updateResult;
            }
            return GetSettings();
        }
    }
}