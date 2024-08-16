using System.Text.Json.Serialization;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi.Api.Media.Providers;

public class MediaProvidersResponse
{
    public MediaContainer MediaContainer { get; set; }
}

public class Action
{
    public string Id { get; set; }
    public string Key { get; set; }
}

public class Directory
{
    public string HubKey { get; set; }
    public string Title { get; set; }
    public string Agent { get; set; }
    public string Language { get; set; }
    public bool? Refreshing { get; set; }
    public string Scanner { get; set; }
    public string Uuid { get; set; }
    public string Id { get; set; }
    public string Key { get; set; }
    public string? Type { get; set; }

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime UpdatedAt { get; set; }

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime ScannedAt { get; set; }

    public List<Pivot> Pivot { get; set; }
}

public class Feature
{
    public string Key { get; set; }
    public string Type { get; set; }
    public List<Directory> Directory { get; set; }
    public List<Action> Action { get; set; }
    public string Flavor { get; set; }
    public string ScrobbleKey { get; set; }
    public string UnscrobbleKey { get; set; }
}

public class MediaContainer
{
    public int? Size { get; set; }
    public bool? AllowCameraUpload { get; set; }
    public bool? AllowChannelAccess { get; set; }
    public bool? AllowSharing { get; set; }
    public bool? AllowSync { get; set; }
    public bool? AllowTuners { get; set; }
    public bool? BackgroundProcessing { get; set; }
    public bool? Certificate { get; set; }
    public bool? CompanionProxy { get; set; }
    public string CountryCode { get; set; }
    public string Diagnostics { get; set; }
    public bool? EventStream { get; set; }
    public string FriendlyName { get; set; }
    public int? Livetv { get; set; }
    public string MachineIdentifier { get; set; }
    public int? MusicAnalysis { get; set; }
    public bool? MyPlex { get; set; }
    public string MyPlexMappingState { get; set; }
    public string MyPlexSigninState { get; set; }
    public bool? MyPlexSubscription { get; set; }
    public string MyPlexUsername { get; set; }
    public int? OfflineTranscode { get; set; }
    public string OwnerFeatures { get; set; }
    public string Platform { get; set; }
    public string PlatformVersion { get; set; }
    public bool? PluginHost { get; set; }
    public bool? PushNotifications { get; set; }
    public bool? ReadOnlyLibraries { get; set; }
    public int? StreamingBrainABRVersion { get; set; }
    public int? StreamingBrainVersion { get; set; }
    public bool? Sync { get; set; }
    public int? TranscoderActiveVideoSessions { get; set; }
    public bool? TranscoderAudio { get; set; }
    public bool? TranscoderLyrics { get; set; }
    public bool? TranscoderSubtitles { get; set; }
    public bool? TranscoderVideo { get; set; }
    public string TranscoderVideoBitrates { get; set; }
    public string TranscoderVideoQualities { get; set; }
    public string TranscoderVideoResolutions { get; set; }

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime? UpdatedAt { get; set; }

    public bool? Updater { get; set; }
    public string Version { get; set; }
    public bool? VoiceSearch { get; set; }
    public List<MediaProvider> MediaProvider { get; set; }
}

public class MediaProvider
{
    public string Identifier { get; set; }
    public string Title { get; set; }
    public string Types { get; set; }
    public string Protocols { get; set; }
    public List<Feature> Feature { get; set; }
}

public class Pivot
{
    public string Id { get; set; }
    public string Key { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Context { get; set; }
    public string Symbol { get; set; }
}
