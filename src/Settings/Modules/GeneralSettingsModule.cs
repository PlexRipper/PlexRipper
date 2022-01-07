using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class GeneralSettingsModule : BaseSettingsModule<IGeneralSettings>, IGeneralSettingsModule
    {
        public bool FirstTimeSetup { get; set; }

        public override string Name => "GeneralSettings";

        protected override IGeneralSettings DefaultValue => new GeneralSettings
        {
            FirstTimeSetup = true,
        };

        public Result Update(IGeneralSettingsModule sourceSettings)
        {
            FirstTimeSetup = sourceSettings.FirstTimeSetup;
            return Result.Ok();
        }

        public Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            var generalSettings = jsonSettings.Value;

            if (generalSettings.TryGetProperty(nameof(FirstTimeSetup), out JsonElement firstTimeSetup))
            {
                FirstTimeSetup = firstTimeSetup.GetBoolean();
            }

            return Result.Ok();
        }

        public override IGeneralSettings GetValues()
        {
            return new GeneralSettings
            {
                FirstTimeSetup = FirstTimeSetup,
            };
        }
    }
}