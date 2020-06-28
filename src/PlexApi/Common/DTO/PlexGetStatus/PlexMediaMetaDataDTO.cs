using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO.PlexGetStatus
{
    /// <summary>
    /// Used to parse the response from the Plex API when requesting a media file to be streamed
    /// E.g.: http://XXX.XXX.XXX.XXX:32400/library/metadata/5516/?X-Plex-Token=tM2sgRsu2NJENK2mqasr
    /// </summary>

    public class PlexMediaMetaDataDTO
    {

        [JsonPropertyName("MediaContainer")]
        public MediaContainerDTO MediaContainerDto { get; set; }
    }

    public class Stream
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("streamType")]
        public int StreamType { get; set; }

        [JsonPropertyName("default")]
        public bool Default { get; set; }

        [JsonPropertyName("codec")]
        public string Codec { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("bitrate")]
        public int Bitrate { get; set; }

        [JsonPropertyName("bitDepth")]
        public int BitDepth { get; set; }

        [JsonPropertyName("chromaLocation")]
        public string ChromaLocation { get; set; }

        [JsonPropertyName("chromaSubsampling")]
        public string ChromaSubsampling { get; set; }

        [JsonPropertyName("codedHeight")]
        public string CodedHeight { get; set; }

        [JsonPropertyName("codedWidth")]
        public string CodedWidth { get; set; }

        [JsonPropertyName("frameRate")]
        public double FrameRate { get; set; }

        [JsonPropertyName("hasScalingMatrix")]
        public bool HasScalingMatrix { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("profile")]
        public string Profile { get; set; }

        [JsonPropertyName("refFrames")]
        public int RefFrames { get; set; }

        [JsonPropertyName("scanType")]
        public string ScanType { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("displayTitle")]
        public string DisplayTitle { get; set; }

        [JsonPropertyName("selected")]
        public bool? Selected { get; set; }

        [JsonPropertyName("channels")]
        public int? Channels { get; set; }

        [JsonPropertyName("audioChannelLayout")]
        public string AudioChannelLayout { get; set; }

        [JsonPropertyName("samplingRate")]
        public int? SamplingRate { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("languageCode")]
        public string LanguageCode { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }
    }

    public class Part
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("audioProfile")]
        public string AudioProfile { get; set; }

        [JsonPropertyName("container")]
        public string Container { get; set; }

        [JsonPropertyName("indexes")]
        public string Indexes { get; set; }

        [JsonPropertyName("videoProfile")]
        public string VideoProfile { get; set; }

        [JsonPropertyName("Stream")]
        public IList<Stream> Stream { get; set; }
    }

    public class Medium
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("duration")]
        public long Duration { get; set; }

        [JsonPropertyName("bitrate")]
        public int Bitrate { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("aspectRatio")]
        public double AspectRatio { get; set; }

        [JsonPropertyName("audioChannels")]
        public int AudioChannels { get; set; }

        [JsonPropertyName("audioCodec")]
        public string AudioCodec { get; set; }

        [JsonPropertyName("videoCodec")]
        public string VideoCodec { get; set; }

        [JsonPropertyName("videoResolution")]
        public string VideoResolution { get; set; }

        [JsonPropertyName("container")]
        public string Container { get; set; }

        [JsonPropertyName("videoFrameRate")]
        public string VideoFrameRate { get; set; }

        [JsonPropertyName("audioProfile")]
        public string AudioProfile { get; set; }

        [JsonPropertyName("videoProfile")]
        public string VideoProfile { get; set; }

        [JsonPropertyName("Part")]
        public IList<Part> Part { get; set; }
    }

    public class Genre
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    public class Director
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    public class Writer
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    public class Producer
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    public class Country
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    public class Role
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("role")]
        public string RoleName { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }
    }

    public class Similar
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    public class Metadata
    {

        [JsonPropertyName("ratingKey")]
        public string RatingKey { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("studio")]
        public string Studio { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonPropertyName("librarySectionID")]
        public int LibrarySectionID { get; set; }

        [JsonPropertyName("librarySectionKey")]
        public string LibrarySectionKey { get; set; }

        [JsonPropertyName("contentRating")]
        public string ContentRating { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("audienceRating")]
        public double AudienceRating { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("tagline")]
        public string Tagline { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonPropertyName("art")]
        public string Art { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [JsonPropertyName("addedAt")]
        public int AddedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonPropertyName("audienceRatingImage")]
        public string AudienceRatingImage { get; set; }

        [JsonPropertyName("ratingImage")]
        public string RatingImage { get; set; }

        [JsonPropertyName("Media")]
        public IList<Medium> Media { get; set; }

        [JsonPropertyName("Genre")]
        public IList<Genre> Genre { get; set; }

        [JsonPropertyName("Director")]
        public IList<Director> Director { get; set; }

        [JsonPropertyName("Writer")]
        public IList<Writer> Writer { get; set; }

        [JsonPropertyName("Producer")]
        public IList<Producer> Producer { get; set; }

        [JsonPropertyName("Country")]
        public IList<Country> Country { get; set; }

        [JsonPropertyName("Role")]
        public IList<Role> Role { get; set; }

        [JsonPropertyName("Similar")]
        public IList<Similar> Similar { get; set; }
    }

    public class MediaContainerDTO
    {

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
        public IList<Metadata> Metadata { get; set; }
    }




}
