using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class GeneralSettingsModule : BaseSettingsModule<IGeneralSettings>, IGeneralSettingsModule
    {
        public bool FirstTimeSetup { get; set; } = true;

        public override string Name => "GeneralSettings";

        public Result Update(IGeneralSettingsModule sourceSettings)
        {
            FirstTimeSetup = sourceSettings.FirstTimeSetup;
            return Result.Ok();
        }

        public void Reset()
        {
            Update(new GeneralSettingsModule());
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