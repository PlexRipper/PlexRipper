using System.Runtime.Serialization;


namespace PlexRipper.Domain
{
    public enum FolderType
    {
        // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
        // Otherwise the Typescript DTO translator in the front-end starts messing up.

        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "DownloadFolder")]
        DownloadFolder = 1,

        [EnumMember(Value = "MovieFolder")]
        MovieFolder = 2,

        [EnumMember(Value = "TvShowFolder")]
        TvShowFolder = 3,

        [EnumMember(Value = "MusicFolder")]
        MusicFolder = 4,

        [EnumMember(Value = "PhotosFolder")]
        PhotosFolder = 5,

        [EnumMember(Value = "OtherVideosFolder")]
        OtherVideosFolder = 6,

        [EnumMember(Value = "GamesVideosFolder")]
        GamesVideosFolder = 7,
    }
}