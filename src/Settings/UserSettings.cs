using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        #region Fields

        private readonly Subject<int> _settingsUpdated = new();

        #endregion

        #region Methods

        #region Public

        public IObservable<int> SettingsUpdated => _settingsUpdated.AsObservable();

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
        public void UpdateSettings(ISettingsModel sourceSettings)
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

            _settingsUpdated.OnNext(0);
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