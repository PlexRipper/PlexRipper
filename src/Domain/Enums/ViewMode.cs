using System.Runtime.Serialization;

namespace PlexRipper.Domain
{
    public enum ViewMode
    {
        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Table")]
        Table = 1,

        [EnumMember(Value = "Poster")]
        Poster = 2,

        [EnumMember(Value = "Overview")]
        Overview = 3,
    }
}