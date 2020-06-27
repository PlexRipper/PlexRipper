using Newtonsoft.Json;
using System.Collections.Generic;

namespace PlexRipper.PlexApi.Common.DTO.PlexGetStatus
{
    /// <summary>
    /// Used to parse the response from the Plex API when requesting a media file to be streamed
    /// E.g.: http://XXX.XXX.XXX.XXX:32400/library/metadata/5516/?X-Plex-Token=tM2sgRsu2NJENK2mqasr
    /// </summary>

    public class PlexMediaMetaDataDTO
    {

        [JsonProperty("MediaContainer")]
        public MediaContainerDTO MediaContainerDto { get; set; }
    }

    public class Stream
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("streamType")]
        public int StreamType { get; set; }

        [JsonProperty("default")]
        public bool Default { get; set; }

        [JsonProperty("codec")]
        public string Codec { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        [JsonProperty("bitDepth")]
        public int BitDepth { get; set; }

        [JsonProperty("chromaLocation")]
        public string ChromaLocation { get; set; }

        [JsonProperty("chromaSubsampling")]
        public string ChromaSubsampling { get; set; }

        [JsonProperty("codedHeight")]
        public string CodedHeight { get; set; }

        [JsonProperty("codedWidth")]
        public string CodedWidth { get; set; }

        [JsonProperty("frameRate")]
        public double FrameRate { get; set; }

        [JsonProperty("hasScalingMatrix")]
        public bool HasScalingMatrix { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }

        [JsonProperty("refFrames")]
        public int RefFrames { get; set; }

        [JsonProperty("scanType")]
        public string ScanType { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("displayTitle")]
        public string DisplayTitle { get; set; }

        [JsonProperty("selected")]
        public bool? Selected { get; set; }

        [JsonProperty("channels")]
        public int? Channels { get; set; }

        [JsonProperty("audioChannelLayout")]
        public string AudioChannelLayout { get; set; }

        [JsonProperty("samplingRate")]
        public int? SamplingRate { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }

    public class Part
    {

        [JsonProperty("id")]
        public int Id { get; set; }

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
        public IList<Stream> Stream { get; set; }
    }

    public class Medium
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("aspectRatio")]
        public double AspectRatio { get; set; }

        [JsonProperty("audioChannels")]
        public int AudioChannels { get; set; }

        [JsonProperty("audioCodec")]
        public string AudioCodec { get; set; }

        [JsonProperty("videoCodec")]
        public string VideoCodec { get; set; }

        [JsonProperty("videoResolution")]
        public string VideoResolution { get; set; }

        [JsonProperty("container")]
        public string Container { get; set; }

        [JsonProperty("videoFrameRate")]
        public string VideoFrameRate { get; set; }

        [JsonProperty("audioProfile")]
        public string AudioProfile { get; set; }

        [JsonProperty("videoProfile")]
        public string VideoProfile { get; set; }

        [JsonProperty("Part")]
        public IList<Part> Part { get; set; }
    }

    public class Genre
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class Director
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class Writer
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class Producer
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class Country
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class Role
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("role")]
        public string RoleName { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }
    }

    public class Similar
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class Metadata
    {

        [JsonProperty("ratingKey")]
        public string RatingKey { get; set; }

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
        public int LibrarySectionID { get; set; }

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
        public int Year { get; set; }

        [JsonProperty("tagline")]
        public string Tagline { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [JsonProperty("addedAt")]
        public int AddedAt { get; set; }

        [JsonProperty("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonProperty("audienceRatingImage")]
        public string AudienceRatingImage { get; set; }

        [JsonProperty("ratingImage")]
        public string RatingImage { get; set; }

        [JsonProperty("Media")]
        public IList<Medium> Media { get; set; }

        [JsonProperty("Genre")]
        public IList<Genre> Genre { get; set; }

        [JsonProperty("Director")]
        public IList<Director> Director { get; set; }

        [JsonProperty("Writer")]
        public IList<Writer> Writer { get; set; }

        [JsonProperty("Producer")]
        public IList<Producer> Producer { get; set; }

        [JsonProperty("Country")]
        public IList<Country> Country { get; set; }

        [JsonProperty("Role")]
        public IList<Role> Role { get; set; }

        [JsonProperty("Similar")]
        public IList<Similar> Similar { get; set; }
    }

    public class MediaContainerDTO
    {

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("librarySectionID")]
        public int LibrarySectionID { get; set; }

        [JsonProperty("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonProperty("librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }

        [JsonProperty("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonProperty("mediaTagVersion")]
        public int MediaTagVersion { get; set; }

        [JsonProperty("Metadata")]
        public IList<Metadata> Metadata { get; set; }
    }




}
