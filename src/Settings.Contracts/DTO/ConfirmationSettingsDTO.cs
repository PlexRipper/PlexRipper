using Newtonsoft.Json;

namespace Settings.Contracts;

public class ConfirmationSettingsDTO : IConfirmationSettings
{
    [JsonProperty(Required = Required.Always)]
    public bool AskDownloadMovieConfirmation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public bool AskDownloadTvShowConfirmation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public bool AskDownloadSeasonConfirmation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public bool AskDownloadEpisodeConfirmation { get; set; }
}