using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AutoMapper;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        private readonly IMapper _mapper;

        #region Fields

        private readonly Subject<ISettingsModel> _settingsUpdated = new();

        #endregion

        public UserSettings(IMapper mapper)
        {
            _mapper = mapper;
        }

        #region Methods

        #region Public

        public IObservable<ISettingsModel> SettingsUpdated => _settingsUpdated.AsObservable();

        public void Reset()
        {
            Log.Information("Resetting UserSettings");
            UpdateSettings(new SettingsModel());
        }

        /// <inheritdoc/>
        public Result<string> GetJsonSettingsObject()
        {
            try
            {
                return Result.Ok();
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        /// <inheritdoc/>
        public Result<ISettingsModel> UpdateSettings(ISettingsModel sourceSettings)
        {
            FirstTimeSetup = sourceSettings.FirstTimeSetup;
            ActiveAccountId = sourceSettings.ActiveAccountId;
            DownloadSegments = sourceSettings.DownloadSegments;

            Language = sourceSettings.Language;
            AskDownloadMovieConfirmation = sourceSettings.AskDownloadMovieConfirmation;
            AskDownloadTvShowConfirmation = sourceSettings.AskDownloadTvShowConfirmation;
            AskDownloadSeasonConfirmation = sourceSettings.AskDownloadSeasonConfirmation;
            AskDownloadEpisodeConfirmation = sourceSettings.AskDownloadEpisodeConfirmation;

            TvShowViewMode = sourceSettings.TvShowViewMode;
            MovieViewMode = sourceSettings.MovieViewMode;

            ShortDateFormat = sourceSettings.ShortDateFormat;
            LongDateFormat = sourceSettings.LongDateFormat;
            TimeFormat = sourceSettings.TimeFormat;
            TimeZone = sourceSettings.TimeZone;
            ShowRelativeDates = sourceSettings.ShowRelativeDates;

            var settingsModel = _mapper.Map<ISettingsModel>(this);
            _settingsUpdated.OnNext(settingsModel);

            return Result.Ok(settingsModel);
        }

        #region Helpers

        public int GetDownloadSpeedLimit(string machineIdentifier)
        {
            if (DownloadLimit.TryGetValue(machineIdentifier, out int speedLimit))
            {
                return speedLimit;
            }

            return 0;
        }

        #endregion

        #endregion

        #endregion
    }
}