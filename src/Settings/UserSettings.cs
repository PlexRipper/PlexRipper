using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : IUserSettings
    {
        #region Fields

        private readonly IConfirmationSettingsModule _confirmationSettingsModule;

        private readonly IDateTimeSettingsModule _dateTimeSettingsModule;

        private readonly IDisplaySettingsModule _displaySettingsModule;

        private readonly IDownloadManagerSettingsModule _downloadManagerSettingsModule;

        private readonly IGeneralSettingsModule _generalSettingsModule;

        private readonly ILanguageSettingsModule _languageSettingsModule;

        private readonly IServerSettingsModule _serverSettingsModule;

        private readonly Subject<ISettingsModel> _settingsUpdated = new();

        #endregion

        public UserSettings(
            IConfirmationSettingsModule confirmationSettingsModule,
            IDateTimeSettingsModule dateTimeSettingsModule,
            IDisplaySettingsModule displaySettingsModule,
            IDownloadManagerSettingsModule downloadManagerSettingsModule,
            IGeneralSettingsModule generalSettingsModule,
            ILanguageSettingsModule languageSettingsModule,
            IServerSettingsModule serverSettingsModule)
        {
            _confirmationSettingsModule = confirmationSettingsModule;
            _dateTimeSettingsModule = dateTimeSettingsModule;
            _displaySettingsModule = displaySettingsModule;
            _downloadManagerSettingsModule = downloadManagerSettingsModule;
            _generalSettingsModule = generalSettingsModule;
            _languageSettingsModule = languageSettingsModule;
            _serverSettingsModule = serverSettingsModule;

            // Alert of any module changes
            Observable.Merge(
                    _confirmationSettingsModule.ModuleHasChanged.Select(_ => 1),
                    _dateTimeSettingsModule.ModuleHasChanged.Select(_ => 1),
                    _displaySettingsModule.ModuleHasChanged.Select(_ => 1),
                    _downloadManagerSettingsModule.ModuleHasChanged.Select(_ => 1),
                    _generalSettingsModule.ModuleHasChanged.Select(_ => 1),
                    _languageSettingsModule.ModuleHasChanged.Select(_ => 1),
                    _serverSettingsModule.ModuleHasChanged.Select(_ => 1)
                )
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => _settingsUpdated.OnNext(GetSettingsModel()));
        }

        #region Methods

        #region Public

        public IObservable<ISettingsModel> SettingsUpdated => _settingsUpdated.AsObservable();

        public void Reset()
        {
            Log.Information("Resetting UserSettings");

            _confirmationSettingsModule.Reset();
            _dateTimeSettingsModule.Reset();
            _displaySettingsModule.Reset();
            _downloadManagerSettingsModule.Reset();
            _generalSettingsModule.Reset();
            _languageSettingsModule.Reset();
            _serverSettingsModule.Reset();
        }

        public Result<ISettingsModel> UpdateSettings(ISettingsModel sourceSettings)
        {
            var settings = new SettingsModel
            {
                ConfirmationSettings = _confirmationSettingsModule.Update(sourceSettings.ConfirmationSettings),
                DateTimeSettings = _dateTimeSettingsModule.Update(sourceSettings.DateTimeSettings),
                DisplaySettings = _displaySettingsModule.Update(sourceSettings.DisplaySettings),
                DownloadManagerSettings = _downloadManagerSettingsModule.Update(sourceSettings.DownloadManagerSettings),
                GeneralSettings = _generalSettingsModule.Update(sourceSettings.GeneralSettings),
                LanguageSettings = _languageSettingsModule.Update(sourceSettings.LanguageSettings),
                ServerSettings = _serverSettingsModule.Update(sourceSettings.ServerSettings),
            };

            return Result.Ok((ISettingsModel)settings);
        }

        public ISettingsModel GetSettingsModel()
        {
            return new SettingsModel
            {
                ConfirmationSettings = _confirmationSettingsModule.GetValues(),
                GeneralSettings = _generalSettingsModule.GetValues(),
                DisplaySettings = _displaySettingsModule.GetValues(),
                LanguageSettings = _languageSettingsModule.GetValues(),
                ServerSettings = _serverSettingsModule.GetValues(),
                DateTimeSettings = _dateTimeSettingsModule.GetValues(),
                DownloadManagerSettings = _downloadManagerSettingsModule.GetValues(),
            };
        }

        public Result SetFromJsonObject(JsonElement settingsJsonElement)
        {
            var results = new List<Result>
            {
                _confirmationSettingsModule.SetFromJson(settingsJsonElement),
                _dateTimeSettingsModule.SetFromJson(settingsJsonElement),
                _displaySettingsModule.SetFromJson(settingsJsonElement),
                _downloadManagerSettingsModule.SetFromJson(settingsJsonElement),
                _generalSettingsModule.SetFromJson(settingsJsonElement),
                _languageSettingsModule.SetFromJson(settingsJsonElement),
                _serverSettingsModule.SetFromJson(settingsJsonElement),
            };

            if (results.Any(x => x.IsFailed))
            {
                return Result.Fail("Failed to set from json object").AddNestedErrors(results.SelectMany(x => x.Errors).ToList());
            }

            return Result.Ok();
        }

        #endregion

        #endregion
    }
}