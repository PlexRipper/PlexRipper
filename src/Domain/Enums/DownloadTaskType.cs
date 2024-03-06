using System.Runtime.Serialization;

namespace PlexRipper.Domain;

public enum DownloadTaskType
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = "None")]
    None = 0,

    /// <summary>
    /// Functions as a wrapper for a MovieData and MoviePart <see cref="DownloadTaskGeneric"/>.
    /// </summary>
    [EnumMember(Value = "Movie")]
    Movie = 1,

    /// <summary>
    /// A movie of a particular quality or version
    /// Doc: https://support.plex.tv/articles/200381043-multi-version-movies/.
    /// </summary>
    [EnumMember(Value = "MovieData")]
    MovieData = 2,

    /// <summary>
    /// A movie that consists of multiple file parts where each file part is a movie part.
    /// </summary>
    [EnumMember(Value = "MoviePart")]
    MoviePart = 3,

    [EnumMember(Value = "TvShow")]
    TvShow = 4,

    [EnumMember(Value = "Season")]
    Season = 5,

    [EnumMember(Value = "Episode")]
    Episode = 6,

    [EnumMember(Value = "EpisodeData")]
    EpisodeData = 7,

    [EnumMember(Value = "EpisodePart")]
    EpisodePart = 8,
}