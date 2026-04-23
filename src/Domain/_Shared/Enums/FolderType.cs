namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FolderType
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up.
    [JsonStringEnumMemberName(nameof(None))]
    None = 0,

    [JsonStringEnumMemberName(nameof(DownloadFolder))]
    DownloadFolder = 1,

    [JsonStringEnumMemberName(nameof(MovieFolder))]
    MovieFolder = 2,

    [JsonStringEnumMemberName(nameof(TvShowFolder))]
    TvShowFolder = 3,

    [JsonStringEnumMemberName(nameof(MusicFolder))]
    MusicFolder = 4,

    [JsonStringEnumMemberName(nameof(PhotosFolder))]
    PhotosFolder = 5,

    [JsonStringEnumMemberName(nameof(OtherVideosFolder))]
    OtherVideosFolder = 6,

    [JsonStringEnumMemberName(nameof(GamesVideosFolder))]
    GamesVideosFolder = 7,

    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 8,
}
