using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PlexRipper.Infrastructure.Common.Models.Plex.PlexLibrary
{
    /// <summary>
    /// Used to parse the response from the Plex API when requesting a media file to be streamed
    /// E.g.: http://XXX.XXX.XXX.XXX:32400/library/metadata/5516/?X-Plex-Token=tM2sgRsu2NJENK2mqasr
    /// </summary>
    public class PlexMediaMetaDataDTO
    {
        [JsonProperty("MediaContainer")]
        public MediaContainer MediaContainer { get; set; }
    }

    public class MediaContainer
    {
        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("librarySectionID")]
        public long LibrarySectionId { get; set; }

        [JsonProperty("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonProperty("librarySectionUUID")]
        public Guid LibrarySectionUuid { get; set; }

        [JsonProperty("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonProperty("mediaTagVersion")]
        public long MediaTagVersion { get; set; }

        [JsonProperty("Metadata")]
        public List<Metadatum> Metadata { get; set; }
    }

    public class Metadatum
    {
        [JsonProperty("ratingKey")]
        public long RatingKey { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("studio")]
        public string Studio { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonProperty("librarySectionID")]
        public long LibrarySectionId { get; set; }

        [JsonProperty("librarySectionKey")]
        public string LibrarySectionKey { get; set; }

        [JsonProperty("contentRating")]
        public string ContentRating { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("audienceRating")]
        public double AudienceRating { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("tagline")]
        public string Tagline { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("originallyAvailableAt")]
        public DateTimeOffset OriginallyAvailableAt { get; set; }

        [JsonProperty("addedAt")]
        public long AddedAt { get; set; }

        [JsonProperty("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonProperty("audienceRatingImage")]
        public string AudienceRatingImage { get; set; }

        [JsonProperty("ratingImage")]
        public string RatingImage { get; set; }

        [JsonProperty("Media")]
        public List<Media> Media { get; set; }

        [JsonProperty("Genre")]
        public List<Country> Genre { get; set; }

        [JsonProperty("Director")]
        public List<Country> Director { get; set; }

        [JsonProperty("Writer")]
        public List<Country> Writer { get; set; }

        [JsonProperty("Producer")]
        public List<Country> Producer { get; set; }

        [JsonProperty("Country")]
        public List<Country> Country { get; set; }

        [JsonProperty("Role")]
        public List<Role> Role { get; set; }

        [JsonProperty("Similar")]
        public List<Country> Similar { get; set; }
    }

    public class Country
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class Media
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("bitrate")]
        public long Bitrate { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("aspectRatio")]
        public double AspectRatio { get; set; }

        [JsonProperty("audioChannels")]
        public long AudioChannels { get; set; }

        [JsonProperty("audioCodec")]
        public string AudioCodec { get; set; }

        [JsonProperty("videoCodec")]
        public string VideoCodec { get; set; }

        [JsonProperty("videoResolution")]
        public long VideoResolution { get; set; }

        [JsonProperty("container")]
        public string Container { get; set; }

        [JsonProperty("videoFrameRate")]
        public string VideoFrameRate { get; set; }

        [JsonProperty("audioProfile")]
        public string AudioProfile { get; set; }

        [JsonProperty("videoProfile")]
        public string VideoProfile { get; set; }

        [JsonProperty("Part")]
        public List<Part> Part { get; set; }
    }

    public class Part
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("audioProfile")]
        public string AudioProfile { get; set; }

        [JsonProperty("container")]
        public string Container { get; set; }

        [JsonProperty("indexes")]
        public string Indexes { get; set; }

        [JsonProperty("videoProfile")]
        public string VideoProfile { get; set; }

        [JsonProperty("Stream")]
        public List<Stream> Stream { get; set; }
    }

    public class Stream
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("streamType")]
        public long StreamType { get; set; }

        [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Default { get; set; }

        [JsonProperty("codec")]
        public string Codec { get; set; }

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public long? Index { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public long? Bitrate { get; set; }

        [JsonProperty("bitDepth", NullValueHandling = NullValueHandling.Ignore)]
        public long? BitDepth { get; set; }

        [JsonProperty("chromaLocation", NullValueHandling = NullValueHandling.Ignore)]
        public string ChromaLocation { get; set; }

        [JsonProperty("chromaSubsampling", NullValueHandling = NullValueHandling.Ignore)]
        public string ChromaSubsampling { get; set; }

        [JsonProperty("codedHeight", NullValueHandling = NullValueHandling.Ignore)]
        public long? CodedHeight { get; set; }

        [JsonProperty("codedWidth", NullValueHandling = NullValueHandling.Ignore)]
        public long? CodedWidth { get; set; }

        [JsonProperty("frameRate", NullValueHandling = NullValueHandling.Ignore)]
        public double? FrameRate { get; set; }

        [JsonProperty("hasScalingMatrix", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasScalingMatrix { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public long? Level { get; set; }

        [JsonProperty("profile", NullValueHandling = NullValueHandling.Ignore)]
        public string Profile { get; set; }

        [JsonProperty("refFrames", NullValueHandling = NullValueHandling.Ignore)]
        public long? RefFrames { get; set; }

        [JsonProperty("scanType", NullValueHandling = NullValueHandling.Ignore)]
        public string ScanType { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("displayTitle")]
        public string DisplayTitle { get; set; }

        [JsonProperty("selected", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Selected { get; set; }

        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public long? Channels { get; set; }

        [JsonProperty("audioChannelLayout", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioChannelLayout { get; set; }

        [JsonProperty("samplingRate", NullValueHandling = NullValueHandling.Ignore)]
        public long? SamplingRate { get; set; }

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public string Language { get; set; }

        [JsonProperty("languageCode", NullValueHandling = NullValueHandling.Ignore)]
        public string LanguageCode { get; set; }

        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; set; }
    }

    public class Role
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("role")]
        public string RoleRole { get; set; }

        [JsonProperty("thumb", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Thumb { get; set; }
    }
}
