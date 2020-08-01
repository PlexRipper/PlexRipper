using System.Collections.Generic;
using System.Text.Json.Serialization;
using Plex.Api.Helpers;

namespace PlexRipper.PlexApi.Models.Status
{
    public class SessionWrapper
    {
        [JsonPropertyName("MediaContainer")]
        public SessionContainer SessionContainer { get; set; }
    }

    public class SessionContainer
    {
        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("Metadata")]
        public List<Session> Sessions { get; set; }
    }

    public class Session
    {
        [JsonPropertyName("addedAt")]
        public string AddedAt { get; set; }

        [JsonPropertyName("art")]
        public string Art { get; set; }

        [JsonPropertyName("contentRating")]
        public string ContentRating { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("grandparentArt")]
        public string GrandparentArt { get; set; }

        [JsonPropertyName("grandparentGuid")]
        public string GrandparentGuid { get; set; }

        [JsonPropertyName("grandparentKey")]
        public string GrandparentKey { get; set; }

        [JsonPropertyName("grandparentRatingKey")]
        public string GrandparentRatingKey { get; set; }

        [JsonPropertyName("grandparentTheme")]
        public string GrandparentTheme { get; set; }

        [JsonPropertyName("grandparentThumb")]
        public string GrandparentThumb { get; set; }

        [JsonPropertyName("grandparentTitle")]
        public string GrandparentTitle { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("index")]
        public string Index { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("lastViewedAt")]
        public string LastViewedAt { get; set; }

        [JsonPropertyName("librarySectionID")]
        public string LibrarySectionId { get; set; }

        [JsonPropertyName("librarySectionKey")]
        public string LibrarySectionKey { get; set; }

        [JsonPropertyName("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonPropertyName("originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [JsonPropertyName("parentGuid")]
        public string ParentGuid { get; set; }

        [JsonPropertyName("parentIndex")]
        public string ParentIndex { get; set; }

        [JsonPropertyName("parentKey")]
        public string ParentKey { get; set; }

        [JsonPropertyName("parentRatingKey")]
        public string ParentRatingKey { get; set; }

        [JsonPropertyName("parentThumb")]
        public string ParentThumb { get; set; }

        [JsonPropertyName("parentTitle")]
        public string ParentTitle { get; set; }

        [JsonPropertyName("rating")]
        public string Rating { get; set; }

        [JsonPropertyName("ratingKey")]
        public string RatingKey { get; set; }

        [JsonPropertyName("sessionKey")]
        public string SessionKey { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("viewOffset")]
        public string ViewOffset { get; set; }

        [JsonPropertyName("year")]
        [JsonConverter(typeof(IntValueConverter))]
        public int Year { get; set; }
        
        [JsonPropertyName("Director")]
        public List<Director> Directors { get; set; }

        [JsonPropertyName("Writer")]
        public List<Writer> Writers { get; set; }

        [JsonPropertyName("Media")]
        public List<Medium> Media { get; set; }

        [JsonPropertyName("User")]
        public PlexUser PlexUser { get; set; }

        [JsonPropertyName("Player")]
        public Player Player { get; set; }

        [JsonPropertyName("Session")]
        public SessionDetail SessionDetail { get; set; }

        [JsonPropertyName("TranscodeSession")]
        public TranscodeSession TranscodeSession { get; set; }
    }
   
    public class Player
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("device")]
        public string Device { get; set; }

        [JsonPropertyName("machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("platformVersion")]
        public string PlatformVersion { get; set; }

        [JsonPropertyName("product")]
        public string Product { get; set; }

        [JsonPropertyName("profile")]
        public string Profile { get; set; }

        [JsonPropertyName("remotePublicAddress")]
        public string RemotePublicAddress { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("vendor")]
        public string Vendor { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("local")]
        public bool Local { get; set; }

        [JsonPropertyName("relayed")]
        public bool Relayed { get; set; }

        [JsonPropertyName("secure")]
        public bool Secure { get; set; }

        [JsonPropertyName("userID")]
        public long UserId { get; set; }
    }

    public class SessionDetail
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("bandwidth")]
        public long Bandwidth { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }
    }

    public class TranscodeSession
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("throttled")]
        public bool Throttled { get; set; }

        [JsonPropertyName("complete")]
        public bool Complete { get; set; }

        [JsonConverter(typeof(DoubleValueConverter))]
        [JsonPropertyName("progress")]
        public double Progress { get; set; }

        [JsonConverter(typeof(DoubleValueConverter))]
        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        [JsonConverter(typeof(LongValueConverter))]
        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("context")]
        public string Context { get; set; }

        [JsonPropertyName("sourceVideoCodec")]
        public string SourceVideoCodec { get; set; }

        [JsonPropertyName("sourceAudioCodec")]
        public string SourceAudioCodec { get; set; }

        [JsonPropertyName("videoDecision")]
        public string VideoDecision { get; set; }

        [JsonPropertyName("audioDecision")]
        public string AudioDecision { get; set; }

        [JsonPropertyName("protocol")]
        public string Protocol { get; set; }

        [JsonPropertyName("container")]
        public string Container { get; set; }

        [JsonPropertyName("videoCodec")]
        public string VideoCodec { get; set; }

        [JsonPropertyName("audioCodec")]
        public string AudioCodec { get; set; }

        [JsonPropertyName("audioChannels")]
        public long AudioChannels { get; set; }

        [JsonPropertyName("transcodeHwRequested")]
        public bool TranscodeHwRequested { get; set; }

        [JsonPropertyName("transcodeHwFullPipeline")]
        public bool TranscodeHwFullPipeline { get; set; }

        [JsonPropertyName("timeStamp")]
        public string TimeStamp { get; set; }

        [JsonPropertyName("maxOffsetAvailable")]
        public string MaxOffsetAvailable { get; set; }

        [JsonPropertyName("minOffsetAvailable")]
        public string MinOffsetAvailable { get; set; }
    }
}