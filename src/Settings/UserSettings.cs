using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using AutoMapper;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Domain.Config;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        private readonly IMapper _mapper;

        #region Fields

        private readonly Subject<ISettingsModel> _settingsUpdated = new();

        private readonly Subject<DownloadSpeedLimitModel> _downloadSpeedLimits = new();

        #endregion

        public UserSettings(IMapper mapper)
        {
            _mapper = mapper;
        }

        #region Methods

        #region Public

        public IObservable<ISettingsModel> SettingsUpdated => _settingsUpdated.AsObservable();

        public IObservable<DownloadSpeedLimitModel> DownloadSpeedLimitUpdated => _downloadSpeedLimits.AsObservable();

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
                return Result.Ok(JsonSerializer.Serialize(GetJsonObject(), DefaultJsonSerializerOptions.Config));
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
            DownloadSpeedLimit = sourceSettings.DownloadSpeedLimit;
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
            return DownloadSpeedLimit?.FirstOrDefault(x => x.MachineIdentifier == machineIdentifier)?.DownloadSpeedLimit ?? 0;
        }

        public int GetDownloadSpeedLimit(int plexServerId)
        {
            return DownloadSpeedLimit?.FirstOrDefault(x => x.PlexServerId == plexServerId)?.DownloadSpeedLimit ?? 0;
        }

        public void SetDownloadSpeedLimit(DownloadSpeedLimitModel downloadSpeedLimit)
        {
            var index = DownloadSpeedLimit
                .FindIndex(x => x.PlexServerId == downloadSpeedLimit.PlexServerId &&
                                x.MachineIdentifier == downloadSpeedLimit.MachineIdentifier);
            if (index > -1)
            {
                if (DownloadSpeedLimit[index].DownloadSpeedLimit != downloadSpeedLimit.DownloadSpeedLimit)
                {
                    DownloadSpeedLimit[index].DownloadSpeedLimit = downloadSpeedLimit.DownloadSpeedLimit;
                    _downloadSpeedLimits.OnNext(DownloadSpeedLimit[index]);
                }
            }
            else
            {
                DownloadSpeedLimit.Add(downloadSpeedLimit);
                _downloadSpeedLimits.OnNext(DownloadSpeedLimit.Last());
            }

            EmitSettingsUpdated();
        }

        private void EmitSettingsUpdated()
        {
            var settingsModel = _mapper.Map<ISettingsModel>(this);
            _settingsUpdated.OnNext(settingsModel);
        }

        #endregion

        #endregion

        #endregion
    }
}