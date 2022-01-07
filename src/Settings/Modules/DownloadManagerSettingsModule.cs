using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class DownloadManagerSettingsModule : BaseSettingsModule<IDownloadManagerSettings>, IDownloadManagerSettingsModule
    {
        public int DownloadSegments { get; set; }

        public override string Name => "DownloadManagerSettings";

        protected override IDownloadManagerSettings DefaultValue => new DownloadManagerSettings
        {
            DownloadSegments = 4,
        };

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