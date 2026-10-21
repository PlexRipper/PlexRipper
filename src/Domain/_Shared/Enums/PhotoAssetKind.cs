namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PhotoAssetKind
{
    Image = 0,
    VideoClip = 1,
}
