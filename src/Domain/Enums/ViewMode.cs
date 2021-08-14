using System.Runtime.Serialization;

namespace PlexRipper.Domain
{
    public enum ViewMode
    {
        [EnumMember(Value = "Table")]
        Table = 0,

        [EnumMember(Value = "Poster")]
        Poster = 1,

        [EnumMember(Value = "Overview")]
        Overview = 2,
    }
}