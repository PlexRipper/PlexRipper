using Newtonsoft.Json;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Common.DTO;

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