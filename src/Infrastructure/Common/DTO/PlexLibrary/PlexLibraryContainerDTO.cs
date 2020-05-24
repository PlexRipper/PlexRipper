using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.DTO.PlexLibrary
{
    public class PlexLibraryContainerDTO
    {
        [JsonProperty("MediaContainer")]
        public PlexMediaContainerDTO MediaContainer { get; set; }

    }
    public class PlexMediaContainerDTO
    {
        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("allowCameraUpload")]
        public bool AllowCameraUpload { get; set; }

        [JsonProperty("allowChannelAccess")]
        public bool AllowChannelAccess { get; set; }

        [JsonProperty("allowMediaDeletion")]
        public bool AllowMediaDeletion { get; set; }

        [JsonProperty("allowSharing")]
        public bool AllowSharing { get; set; }

        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("backgroundProcessing")]
        public bool BackgroundProcessing { get; set; }

        [JsonProperty("certificate")]
        public bool Certificate { get; set; }

        [JsonProperty("companionProxy")]
        public bool CompanionProxy { get; set; }

        [JsonProperty("countryCode")]
        public object CountryCode { get; set; }

        [JsonProperty("diagnostics")]
        public object Diagnostics { get; set; }

        [JsonProperty("eventStream")]
        public bool EventStream { get; set; }

        [JsonProperty("friendlyName")]
        public object FriendlyName { get; set; }

        [JsonProperty("hubSearch")]
        public bool HubSearch { get; set; }

        [JsonProperty("itemClusters")]
        public bool ItemClusters { get; set; }

        [JsonProperty("machineIdentifier")]
        public object MachineIdentifier { get; set; }

        [JsonProperty("mediaProviders")]
        public bool MediaProviders { get; set; }

        [JsonProperty("multiuser")]
        public bool Multiuser { get; set; }

        [JsonProperty("myPlex")]
        public bool MyPlex { get; set; }

        [JsonProperty("myPlexMappingState")]
        public object MyPlexMappingState { get; set; }

        [JsonProperty("myPlexSigninState")]
        public object MyPlexSigninState { get; set; }

        [JsonProperty("myPlexSubscription")]
        public bool MyPlexSubscription { get; set; }

        [JsonProperty("myPlexUsername")]
        public object MyPlexUsername { get; set; }

        [JsonProperty("photoAutoTag")]
        public bool PhotoAutoTag { get; set; }

        [JsonProperty("platform")]
        public object Platform { get; set; }

        [JsonProperty("platformVersion")]
        public object PlatformVersion { get; set; }

        [JsonProperty("pluginHost")]
        public bool PluginHost { get; set; }

        [JsonProperty("readOnlyLibraries")]
        public bool ReadOnlyLibraries { get; set; }

        [JsonProperty("requestParametersInCookie")]
        public bool RequestParametersInCookie { get; set; }

        [JsonProperty("streamingBrainVersion")]
        public int StreamingBrainVersion { get; set; }

        [JsonProperty("sync")]
        public bool Sync { get; set; }

        [JsonProperty("transcoderActiveVideoSessions")]
        public int TranscoderActiveVideoSessions { get; set; }

        [JsonProperty("transcoderAudio")]
        public bool TranscoderAudio { get; set; }

        [JsonProperty("transcoderLyrics")]
        public bool TranscoderLyrics { get; set; }

        [JsonProperty("transcoderPhoto")]
        public bool TranscoderPhoto { get; set; }

        [JsonProperty("transcoderSubtitles")]
        public bool TranscoderSubtitles { get; set; }

        [JsonProperty("transcoderVideo")]
        public bool TranscoderVideo { get; set; }

        [JsonProperty("transcoderVideoBitrates")]
        public object TranscoderVideoBitrates { get; set; }

        [JsonProperty("transcoderVideoQualities")]
        public object TranscoderVideoQualities { get; set; }

        [JsonProperty("transcoderVideoResolutions")]
        public object TranscoderVideoResolutions { get; set; }

        [JsonProperty("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonProperty("updater")]
        public bool Updater { get; set; }

        [JsonProperty("version")]
        public object Version { get; set; }

        [JsonProperty("voiceSearch")]
        public bool VoiceSearch { get; set; }

        [JsonProperty("directory")]
        public PlexLibraryDirectoryDTO[] Directory { get; set; }
    }

    public class PlexLibraryDirectoryDTO
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

}
