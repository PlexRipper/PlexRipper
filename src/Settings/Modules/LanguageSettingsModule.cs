using System;
using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class LanguageSettingsModule : BaseSettingsModule<ILanguageSettings>, ILanguageSettingsModule
    {
        #region Properties

        public string Language { get; set; } = "en-US";

        public override string Name => "LanguageSettings";

        #endregion

        #region Public Methods

        public Result Update(ILanguageSettings sourceSettings)
        {
            var hasChanged = false;
            if (Language != sourceSettings.Language)
            {
                Language = sourceSettings.Language;
                hasChanged = true;
            }

            if (hasChanged)
            {
                EmitModuleHasChanged(GetValues());
            }

            return Result.Ok();
        }

        public void Reset()
        {
            Update(new LanguageSettingsModule());
        }

        public Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            var languageSettings = jsonSettings.Value;

            if (languageSettings.TryGetProperty(nameof(Language), out JsonElement language))
            {
                Language = language.GetString();
            }

            return Result.Ok();
        }

        public ILanguageSettings GetValues()
        {
            return new LanguageSettings
            {
                Language = Language,
            };
        }

        public Result Update(ILanguageSettingsModule sourceSettings)
        {
            Language = sourceSettings.Language;
            return Result.Ok();
        }

        #endregion
    }
}