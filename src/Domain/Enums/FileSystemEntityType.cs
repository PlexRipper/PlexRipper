using System.Runtime.Serialization;

namespace PlexRipper.Domain;

public enum FileSystemEntityType
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = "Parent")]
    Parent = 0,

    [EnumMember(Value = "Drive")]
    Drive = 1,

    [EnumMember(Value = "Folder")]
    Folder = 2,

    [EnumMember(Value = "File")]
    File = 3,
}