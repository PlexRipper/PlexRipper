namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PhotoItemKind
{
    /// <summary>
    /// A photo folder.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Folder))]
    Folder = 0,

    /// <summary>
    /// A metadata photo album.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Album))]
    Album = 1,

    /// <summary>
    /// A photo asset.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Asset))]
    Asset = 2,
}
