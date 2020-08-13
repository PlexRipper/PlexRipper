using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models
{
    /// <summary>
    /// Attributes:
    ///     part (:class:`~Plex.Api.Models.Part`): Media part this stream belongs to.
    ///     codec (str): Codec of this stream (ex: srt, ac3, mpeg4).
    ///     codecID (str): Codec ID (ex: XVID).
    ///     id (int): Unique stream ID on this server.
    ///     index (int): Unknown
    ///     language (str): Stream language (ex: English, ไทย).
    ///     languageCode (str): Ascii code for language (ex: eng, tha).
    ///     selected (bool): True if this stream is selected.
    ///     streamType (int): Stream type (1=:class:`~plexapi.media.VideoStream`,
    ///     2=:class:`~plexapi.media.AudioStream`, 3=:class:`~plexapi.media.SubtitleStream`).
    ///     type (int): Alias for streamType.
    /// </summary>
    public class Stream
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(LongValueConverter))]
        public long Id { get; set; }

        [JsonPropertyName("streamType")]
        [JsonConverter(typeof(IntValueConverter))]
        public int StreamType { get; set; }

        [JsonPropertyName("default")]
        public bool Default { get; set; }

        [JsonPropertyName("Codec")]
        public string Codec { get; set; }

        [JsonPropertyName("index")]
        [JsonConverter(typeof(LongValueConverter))]
        public long Index { get; set; }

        [JsonPropertyName("bitrate")]
        [JsonConverter(typeof(LongValueConverter))]
        public long Bitrate { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("languageCode")]
        public string LanguageCode { get; set; }

        [JsonPropertyName("anamorphic")]
        public bool Anamorphic { get; set; }

        [JsonPropertyName("bitDepth")]
        [JsonConverter(typeof(LongValueConverter))]
        public long BitDepth { get; set; }

        [JsonPropertyName("chromaLocation")]
        public string ChromaLocation { get; set; }

        [JsonPropertyName("chromaSubsampling")]
        public string ChromaSubsampling { get; set; }

        [JsonPropertyName("colorPrimaries")]
        public string ColorPrimaries { get; set; }

        [JsonPropertyName("codedHeight")]
        [JsonConverter(typeof(IntValueConverter))]
        public int CodedHeight { get; set; }

        [JsonPropertyName("codedWidth")]
        [JsonConverter(typeof(IntValueConverter))]
        public int CodedWidth { get; set; }

        [JsonPropertyName("frameRate")]
        [JsonConverter(typeof(DoubleValueConverter))]
        public double FrameRate { get; set; }

        [JsonPropertyName("hasScalingMatrix")]
        [JsonConverter(typeof(BooleanValueConverter))]
        public bool HasScalingMatrix { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("pixelAspectRatio")]
        public string PixelAspectRatio { get; set; }

        [JsonPropertyName("profile")]
        public string Profile { get; set; }

        [JsonPropertyName("refFrames")]
        public int RefFrames { get; set; }

        [JsonPropertyName("scanType")]
        public string ScanType { get; set; }

        [JsonPropertyName("width")]
        [JsonConverter(typeof(IntValueConverter))]
        public int Width { get; set; }

        [JsonPropertyName("displayTitle")]
        public string DisplayTitle { get; set; }

        [JsonPropertyName("extendedDisplayTitle")]
        public string ExtendedDisplayTitle { get; set; }

        // Unconfirmed from here
        [JsonPropertyName("colorRange")]
        public string ColorRange { get; set; }

        [JsonPropertyName("colorSpace")]
        public string ColorSpace { get; set; }

        [JsonPropertyName("colorTrc")]
        public string ColorTrc { get; set; }


        [JsonPropertyName("streamIdentifier")]
        public string StreamIdentifier { get; set; }

        [JsonPropertyName("selected")]
        [JsonConverter(typeof(BooleanValueConverter))]
        public bool Selected { get; set; }

        [JsonPropertyName("channels")]
        [JsonConverter(typeof(LongValueConverter))]
        public long Channels { get; set; }

        [JsonPropertyName("samplingRate")]
        [JsonConverter(typeof(LongValueConverter))]
        public long SamplingRate { get; set; }

        [JsonPropertyName("audioChannelLayout")]
        public string AudioChannelLayout { get; set; }

    }
}