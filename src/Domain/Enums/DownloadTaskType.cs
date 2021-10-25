using System.Runtime.Serialization;

namespace PlexRipper.Domain
{
    public enum DownloadTaskType
    {
        // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
        // Otherwise the Typescript DTO translator in the front-end starts messing up
        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Movie")]
        Movie = 1,

        [EnumMember(Value = "MoviePart")]
        MoviePart = 2,

        [EnumMember(Value = "TvShow")]
        TvShow = 3,

        [EnumMember(Value = "Season")]
        Season = 4,

        [EnumMember(Value = "Episode")]
        Episode = 5,

        [EnumMember(Value = "EpisodePart")]
        EpisodePart = 6,
    }
}