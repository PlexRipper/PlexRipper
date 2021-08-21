using System.Runtime.Serialization;

namespace PlexRipper.Domain
{
    public enum FileSystemEntityType
    {
        [EnumMember(Value = "Parent")]
        Parent = 0,

        [EnumMember(Value = "Drive")]
        Drive = 1,

        [EnumMember(Value = "Folder")]
        Folder = 2,

        [EnumMember(Value = "File")]
        File = 3,
    }
}