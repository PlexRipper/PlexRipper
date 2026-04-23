namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileSystemEntityType
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [JsonStringEnumMemberName(nameof(Parent))]
    Parent = 0,

    [JsonStringEnumMemberName(nameof(Drive))]
    Drive = 1,

    [JsonStringEnumMemberName(nameof(Folder))]
    Folder = 2,

    [JsonStringEnumMemberName(nameof(File))]
    File = 3,
}
