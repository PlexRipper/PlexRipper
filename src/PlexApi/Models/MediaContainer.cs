using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Config.Converters;

namespace PlexRipper.PlexApi.Models
{
    public class MediaContainer
    {
        public int TotalSize { get; set; }

        //Movie
        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("allowSync")]
        public bool AllowSync { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("librarySectionID")]
        public int LibrarySectionID { get; set; }

        [JsonPropertyName("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonPropertyName("librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }

        [JsonPropertyName("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonPropertyName("mediaTagVersion")]
        public int MediaTagVersion { get; set; }

        [JsonPropertyName("Metadata")]
        public List<Metadata> Metadata { get; set; }

        //Library Sections
        [JsonPropertyName("art")]
        public string Art { get; set; }

        [JsonPropertyName("nocache")]
        public bool Nocache { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonPropertyName("title1")]
        public string Title1 { get; set; }

        [JsonPropertyName("title2")]
        public string Title2 { get; set; }

        [JsonPropertyName("viewGroup")]
        public string ViewGroup { get; set; }

        [JsonPropertyName("viewMode")]
        public int ViewMode { get; set; }

        //TV Show Seasons
        [JsonPropertyName("banner")]
        public string Banner { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("parentIndex")]
        public int ParentIndex { get; set; }

        [JsonPropertyName("parentTitle")]
        public string ParentTitle { get; set; }

        [JsonPropertyName("parentYear")]
        public int ParentYear { get; set; }

        [JsonPropertyName("sortAsc")]
        public bool SortAsc { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("theme")]
        public string Theme { get; set; }

        /// <summary>
        /// The PlexLibraries are contained here, JsonPropertyName Directory must be capitalized.
        /// </summary>
        [JsonPropertyName("Directory")]
        public List<Directory> Directory { get; set; }

        //TV Show Episode
        [JsonPropertyName("grandparentContentRating")]
        public string GrandparentContentRating { get; set; }

        [JsonPropertyName("grandparentRatingKey")]
        public int GrandparentRatingKey { get; set; }

        [JsonPropertyName("grandparentStudio")]
        public string GrandparentStudio { get; set; }

        [JsonPropertyName("grandparentTheme")]
        public string GrandparentTheme { get; set; }

        [JsonPropertyName("grandparentThumb")]
        public string GrandparentThumb { get; set; }

        [JsonPropertyName("grandparentTitle")]
        public string GrandparentTitle { get; set; }

        //Plex Status
        [JsonPropertyName("allowCameraUpload")]
        public bool AllowCameraUpload { get; set; }

        [JsonPropertyName("allowChannelAccess")]
        public bool AllowChannelAccess { get; set; }

        [JsonPropertyName("allowMediaDeletion")]
        public bool AllowMediaDeletion { get; set; }

        [JsonPropertyName("allowSharing")]
        public bool AllowSharing { get; set; }

        [JsonPropertyName("allowTuners")]
        public bool AllowTuners { get; set; }

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

        [JsonPropertyName("liveTv")]
        public int LiveTv { get; set; }

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

        [JsonPropertyName("ownerFeatures")]
        public string OwnerFeatures { get; set; }

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

        [JsonPropertyName("streamingBrainAbrVersion")]
        public int StreamingBrainAbrVersion { get; set; }

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

        [JsonPropertyName("createdAt")]
        [JsonConverter(typeof(LongToDateTime))]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        [JsonConverter(typeof(LongToDateTime))]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("updater")]
        public bool Updater { get; set; }

        [JsonPropertyName("version")]
        public object Version { get; set; }

        [JsonPropertyName("voiceSearch")]
        public bool VoiceSearch { get; set; }
    }
}