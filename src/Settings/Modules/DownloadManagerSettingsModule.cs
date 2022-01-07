using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class DownloadManagerSettingsModule : BaseSettingsModule<IDownloadManagerSettings>, IDownloadManagerSettingsModule
    {
        public int DownloadSegments { get; set; } = 4;

        public override string Name => "DownloadManagerSettings";

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

        public override IDownloadManagerSettings GetValues()
        {
            return new DownloadManagerSettings
            {
                DownloadSegments = DownloadSegments,
            };
        }
    }
}