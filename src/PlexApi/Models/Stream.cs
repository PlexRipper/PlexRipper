using System.Text.Json.Serialization;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

public class Stream
{
    public long Id { get; set; }


    public int StreamType { get; set; }


    public bool Default { get; set; }


    public string Codec { get; set; }


    public long Index { get; set; }


    public long Bitrate { get; set; }


    public string Language { get; set; }


    public string LanguageCode { get; set; }


    public bool Anamorphic { get; set; }


    public long BitDepth { get; set; }


    public string ChromaLocation { get; set; }


    public string ChromaSubsampling { get; set; }


    public string ColorPrimaries { get; set; }


    [JsonConverter(typeof(IntValueConverter))]
    public int CodedHeight { get; set; }


    [JsonConverter(typeof(IntValueConverter))]
    public int CodedWidth { get; set; }


    [JsonConverter(typeof(DoubleValueConverter))]
    public double FrameRate { get; set; }


    [JsonConverter(typeof(BooleanValueConverter))]
    public bool HasScalingMatrix { get; set; }


    public int Height { get; set; }


    public int Level { get; set; }


    public string PixelAspectRatio { get; set; }


    public string Profile { get; set; }


    public int RefFrames { get; set; }


    public string ScanType { get; set; }


    [JsonConverter(typeof(IntValueConverter))]
    public int Width { get; set; }


    public string DisplayTitle { get; set; }


    public string ExtendedDisplayTitle { get; set; }

    // Unconfirmed from here

    public string ColorRange { get; set; }


    public string ColorSpace { get; set; }


    public string ColorTrc { get; set; }


    public string StreamIdentifier { get; set; }


    [JsonConverter(typeof(BooleanValueConverter))]
    public bool Selected { get; set; }


    public long Channels { get; set; }


    public long SamplingRate { get; set; }


    public string AudioChannelLayout { get; set; }
}