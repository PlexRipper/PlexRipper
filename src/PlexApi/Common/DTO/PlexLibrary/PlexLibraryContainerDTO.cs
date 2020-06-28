using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO.PlexLibrary
{
    public class PlexLibraryContainerDTO
    {
        [JsonPropertyName("MediaContainer")]
        public PlexMediaContainerDTO MediaContainer { get; set; }

    }
    public class PlexMediaContainerDTO
    {
        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("allowCameraUpload")]
        public bool AllowCameraUpload { get; set; }

        [JsonPropertyName("allowChannelAccess")]
        public bool AllowChannelAccess { get; set; }

        [JsonPropertyName("allowMediaDeletion")]
        public bool AllowMediaDeletion { get; set; }

        [JsonPropertyName("allowSharing")]
        public bool AllowSharing { get; set; }

        [JsonPropertyName("allowSync")]
        public bool AllowSync { get; set; }

        [JsonPropertyName("backgroundProcessing")]
        public bool BackgroundProcessing { get; set; }

        [JsonPropertyName("certificate")]
        public bool Certificate { get; set; }

        [JsonPropertyName("companionProxy")]
        public bool CompanionProxy { get; set; }

        [JsonPropertyName("countryCode")]
        public object CountryCode { get; set; }

        [JsonPropertyName("diagnostics")]
        public object Diagnostics { get; set; }

        [JsonPropertyName("eventStream")]
        public bool EventStream { get; set; }

        [JsonPropertyName("friendlyName")]
        public object FriendlyName { get; set; }

        [JsonPropertyName("hubSearch")]
        public bool HubSearch { get; set; }

        [JsonPropertyName("itemClusters")]
        public bool ItemClusters { get; set; }

        [JsonPropertyName("machineIdentifier")]
        public object MachineIdentifier { get; set; }

        [JsonPropertyName("mediaProviders")]
        public bool MediaProviders { get; set; }

        [JsonPropertyName("multiuser")]
        public bool Multiuser { get; set; }

        [JsonPropertyName("myPlex")]
        public bool MyPlex { get; set; }

        [JsonPropertyName("myPlexMappingState")]
        public object MyPlexMappingState { get; set; }

        [JsonPropertyName("myPlexSigninState")]
        public object MyPlexSigninState { get; set; }

        [JsonPropertyName("myPlexSubscription")]
        public bool MyPlexSubscription { get; set; }

        [JsonPropertyName("myPlexUsername")]
        public object MyPlexUsername { get; set; }

        [JsonPropertyName("photoAutoTag")]
        public bool PhotoAutoTag { get; set; }

        [JsonPropertyName("platform")]
        public object Platform { get; set; }

        [JsonPropertyName("platformVersion")]
        public object PlatformVersion { get; set; }

        [JsonPropertyName("pluginHost")]
        public bool PluginHost { get; set; }

        [JsonPropertyName("readOnlyLibraries")]
        public bool ReadOnlyLibraries { get; set; }

        [JsonPropertyName("requestParametersInCookie")]
        public bool RequestParametersInCookie { get; set; }

        [JsonPropertyName("streamingBrainVersion")]
        public int StreamingBrainVersion { get; set; }

        [JsonPropertyName("sync")]
        public bool Sync { get; set; }

        [JsonPropertyName("transcoderActiveVideoSessions")]
        public int TranscoderActiveVideoSessions { get; set; }

        [JsonPropertyName("transcoderAudio")]
        public bool TranscoderAudio { get; set; }

        [JsonPropertyName("transcoderLyrics")]
        public bool TranscoderLyrics { get; set; }

        [JsonPropertyName("transcoderPhoto")]
        public bool TranscoderPhoto { get; set; }

        [JsonPropertyName("transcoderSubtitles")]
        public bool TranscoderSubtitles { get; set; }

        [JsonPropertyName("transcoderVideo")]
        public bool TranscoderVideo { get; set; }

        [JsonPropertyName("transcoderVideoBitrates")]
        public object TranscoderVideoBitrates { get; set; }

        [JsonPropertyName("transcoderVideoQualities")]
        public object TranscoderVideoQualities { get; set; }

        [JsonPropertyName("transcoderVideoResolutions")]
        public object TranscoderVideoResolutions { get; set; }

        [JsonPropertyName("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonPropertyName("updater")]
        public bool Updater { get; set; }

        [JsonPropertyName("version")]
        public object Version { get; set; }

        [JsonPropertyName("voiceSearch")]
        public bool VoiceSearch { get; set; }

        [JsonPropertyName("directory")]
        public PlexLibraryDirectoryDTO[] Directory { get; set; }
    }

    public class PlexLibraryDirectoryDTO
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

}
