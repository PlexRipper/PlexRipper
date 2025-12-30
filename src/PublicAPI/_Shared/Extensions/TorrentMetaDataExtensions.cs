namespace Reaparr.PublicAPI;

public static class TorrentMetaDataExtensions
{
    private static readonly ILogger _log = LogFactory.GetLogger(typeof(TorrentMetaDataExtensions));

    public static TorrentMetadataDTO ToTorrentMetadataDTO(this BencodeNET.Objects.BDictionary dictionary) =>
        new()
        {
            Type = dictionary.GetEnumValue<PlexMediaType>(nameof(TorrentMetadataDTO.Type)),
            MediaId = dictionary.GetIntValue(nameof(TorrentMetadataDTO.MediaId)),
            DataId = dictionary.GetIntValue(nameof(TorrentMetadataDTO.DataId)),
            PartId = dictionary.GetIntValue(nameof(TorrentMetadataDTO.PartId)),
            PartPlexId = dictionary.GetIntValue(nameof(TorrentMetadataDTO.PartPlexId)),
            Quality = dictionary.GetEnumValue<VideoQuality>(nameof(TorrentMetadataDTO.Quality)),
            LibraryId = dictionary.GetIntValue(nameof(TorrentMetadataDTO.LibraryId)),
            ServerId = dictionary.GetIntValue(nameof(TorrentMetadataDTO.ServerId)),
        };

    private static int GetIntValue(this BencodeNET.Objects.BDictionary dictionary, string key)
    {
        if (dictionary.TryGetValue(key, out var value) && int.TryParse(value.ToString(), out var intValue))
            return intValue;

        _log.Here().Warning("Failed to parse integer value for key '{Key}' from BDictionary.", key);
        return -1;
    }

    private static TEnum GetEnumValue<TEnum>(this BencodeNET.Objects.BDictionary dictionary, string key)
        where TEnum : struct
    {
        if (dictionary.TryGetValue(key, out var value) && Enum.TryParse<TEnum>(value.ToString(), out var enumValue))
            return enumValue;

        _log.Here().Warning("Failed to parse enum value for key '{Key}' from BDictionary.", key);
        return default;
    }
}
