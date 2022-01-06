using System.Text.Json;
using FluentResults;
using PlexRipper.Application;

namespace PlexRipper.Settings.Modules
{
    public class DownloadManagerSettingsModule : BaseSettingsModule<IDownloadManagerSettings>, IDownloadManagerSettingsModule
    {
        public int DownloadSegments { get; set; } = 4;

        public override string Name => "DownloadManagerSettings";

        public Result Update(IDownloadManagerSettingsModule sourceSettings)
        {
            DownloadSegments = sourceSettings.DownloadSegments;

            return Result.Ok();
        }

        public Result Update(IDownloadManagerSettings sourceSettings)
        {
            var hasChanged = false;

            if (DownloadSegments != sourceSettings.DownloadSegments)
            {
                DownloadSegments = sourceSettings.DownloadSegments;
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
            Update(new DownloadManagerSettingsModule());
        }

        /// <inheritdoc/>
        public Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            var downloadManagerSettings = jsonSettings.Value;

            if (downloadManagerSettings.TryGetProperty(nameof(DownloadSegments), out JsonElement downloadSegments))
            {
                DownloadSegments = downloadSegments.GetInt32();
            }

            return Result.Ok();
        }

        public IDownloadManagerSettings GetValues()
        {
            throw new System.NotImplementedException();
        }
    }
}