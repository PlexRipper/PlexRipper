using System.Text.Json.Serialization;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi.Models;

public class MediaContainer
{
    public int TotalSize { get; set; }

    //Movie

    public int Size { get; set; }

    public bool AllowSync { get; set; }

    public string Identifier { get; set; }

    public int LibrarySectionID { get; set; }

    public string LibrarySectionTitle { get; set; }

    public string LibrarySectionUUID { get; set; }

    public string MediaTagPrefix { get; set; }

    public int MediaTagVersion { get; set; }

    public List<Metadata> Metadata { get; set; }

    //Library Sections

    public string Art { get; set; }

    public bool Nocache { get; set; }

    public string Thumb { get; set; }

    public string Title1 { get; set; }

    public string Title2 { get; set; }

    public string ViewGroup { get; set; }

    public int ViewMode { get; set; }

    //TV Show Seasons

    public string Banner { get; set; }

    public string Key { get; set; }

    public int ParentIndex { get; set; }

    public string ParentTitle { get; set; }

    public int ParentYear { get; set; }

    public bool SortAsc { get; set; }

    public string Summary { get; set; }

    public string Theme { get; set; }

    /// <summary>
    /// The PlexLibraries are contained here, JsonPropertyName Directory must be capitalized.
    /// </summary>

    public List<Directory> Directory { get; set; }

    //TV Show Episode

    public string GrandparentContentRating { get; set; }

    public int GrandparentRatingKey { get; set; }

    public string GrandparentStudio { get; set; }

    public string GrandparentTheme { get; set; }

    public string GrandparentThumb { get; set; }

    public string GrandparentTitle { get; set; }

    //Plex Status

    public bool AllowCameraUpload { get; set; }

    public bool AllowChannelAccess { get; set; }

    public bool AllowMediaDeletion { get; set; }

    public bool AllowSharing { get; set; }

    public bool AllowTuners { get; set; }

    public bool BackgroundProcessing { get; set; }

    public bool Certificate { get; set; }

    public bool CompanionProxy { get; set; }

    public object CountryCode { get; set; }

    public object Diagnostics { get; set; }

    public bool EventStream { get; set; }

    public object FriendlyName { get; set; }

    public bool HubSearch { get; set; }

    public bool ItemClusters { get; set; }

    public int LiveTv { get; set; }

    public object MachineIdentifier { get; set; }

    public bool MediaProviders { get; set; }

    public bool Multiuser { get; set; }

    public bool MyPlex { get; set; }

    public object MyPlexMappingState { get; set; }

    public object MyPlexSigninState { get; set; }

    public bool MyPlexSubscription { get; set; }

    public object MyPlexUsername { get; set; }

    public string OwnerFeatures { get; set; }

    public bool PhotoAutoTag { get; set; }

    public object Platform { get; set; }

    public object PlatformVersion { get; set; }

    public bool PluginHost { get; set; }

    public bool ReadOnlyLibraries { get; set; }

    public bool RequestParametersInCookie { get; set; }

    public int StreamingBrainVersion { get; set; }

    public int StreamingBrainAbrVersion { get; set; }

    public bool Sync { get; set; }

    public int TranscoderActiveVideoSessions { get; set; }

    public bool TranscoderAudio { get; set; }

    public bool TranscoderLyrics { get; set; }

    public bool TranscoderPhoto { get; set; }

    public bool TranscoderSubtitles { get; set; }

    public bool TranscoderVideo { get; set; }

    public object TranscoderVideoBitrates { get; set; }

    public object TranscoderVideoQualities { get; set; }

    public object TranscoderVideoResolutions { get; set; }

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime CreatedAt { get; set; }

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime UpdatedAt { get; set; }

    public bool Updater { get; set; }

    public object Version { get; set; }

    public bool VoiceSearch { get; set; }
}
