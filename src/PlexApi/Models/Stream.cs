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
        [JsonConverter(typeof(LongValueConverter))]
        public long Id { get; set; }
        
        [JsonConverter(typeof(IntValueConverter))]
        public int StreamType { get; set; }
        
        [JsonPropertyName("_default")]
        public bool Default { get; set; }
        
        public string Codec { get; set; }
        
        [JsonConverter(typeof(LongValueConverter))]
        public long Index { get; set; }
        
        [JsonConverter(typeof(LongValueConverter))]
        public long Bitrate { get; set; }
        
        [JsonConverter(typeof(LongValueConverter))]
        public long BitDepth { get; set; }

        public string ChromaLocation { get; set; }

        public string ChromaSubsampling { get; set; }

        public string ColorPrimaries { get; set; }
 
        public string ColorRange { get; set; }
        
        public string ColorSpace { get; set; }
        
        public string ColorTrc { get; set; }
        
        [JsonConverter(typeof(DoubleValueConverter))]
        public double FrameRate { get; set; }
        
        [JsonConverter(typeof(BooleanValueConverter))]
        public bool HasScalingMatrix { get; set; }
        
        [JsonConverter(typeof(LongValueConverter))]
        public long Height { get; set; }
        
        [JsonConverter(typeof(IntValueConverter))]
        public int Level { get; set; }
        
        public string Profile { get; set; }
        
        [JsonConverter(typeof(LongValueConverter))]
        public long RefFrames { get; set; }
        
        public string ScanType { get; set; }
        
        public string StreamIdentifier { get; set; }

        [JsonConverter(typeof(IntValueConverter))]
        public int Width { get; set; }

        public string DisplayTitle { get; set; }
        
        [JsonConverter(typeof(BooleanValueConverter))]
        public bool Selected { get; set; }
        
        [JsonConverter(typeof(LongValueConverter))]
        public long Channels { get; set; }
        
        public string Language { get; set; }
        
        public string LanguageCode { get; set; }
        
        [JsonConverter(typeof(LongValueConverter))]
        public long SamplingRate { get; set; }
        
        public string AudioChannelLayout { get; set; }

        
        // Tv Episode
        public bool Anamorphic { get; set; }
        public string PixelAspectRatio { get; set; }
    }
}