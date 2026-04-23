namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadTaskPhase
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [JsonStringEnumMemberName(nameof(None))]
    None = 0,

    [JsonStringEnumMemberName(nameof(Downloading))]
    Downloading = 1,

    [JsonStringEnumMemberName(nameof(FileTransfer))]
    FileTransfer = 2,

    [JsonStringEnumMemberName(nameof(Completed))]
    Completed = 3,

    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 4,
}
