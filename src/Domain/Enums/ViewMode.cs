using System.Runtime.Serialization;

namespace PlexRipper.Domain;

public enum ViewMode
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = "Table")]
    Table = 0,

    [EnumMember(Value = "Poster")]
    Poster = 1,
}