using Newtonsoft.Json;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class SettingsModelDTO : ISettingsModel
    {
        [JsonProperty(Required = Required.Always)]
        public bool FirstTimeSetup { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int ActiveAccountId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int DownloadSegments { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadMovieConfirmation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadTvShowConfirmation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadSeasonConfirmation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool AskDownloadEpisodeConfirmation { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ViewMode TvShowViewMode { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ViewMode MovieViewMode { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ShortDateFormat { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string LongDateFormat { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TimeFormat { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TimeZone { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool ShowRelativeDates { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Language { get; set; }

    }
}