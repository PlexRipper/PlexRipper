using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class LanguageSettingsModule : BaseSettingsModule<ILanguageSettings>, ILanguageSettingsModule
    {
        #region Properties

        public string Language { get; set; }

        public override string Name => "LanguageSettings";

        public override ILanguageSettings DefaultValues()
        {
            return new LanguageSettings
            {
                Language = "en-US",
            };
        }

        #endregion

        #region Public Methods

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

        public override ILanguageSettings GetValues()
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