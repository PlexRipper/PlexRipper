namespace Reaparr.Domain;

public enum PlexGenreType
{
    /// <summary>
    /// Represents an unknown or unspecified genre type.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Unknown))]
    Unknown = 0,

    /// <summary>
    /// Represents a group of genres or a general category that encompasses multiple specific genres.
    /// </summary>
    [JsonStringEnumMemberName(nameof(Group))]
    Group = 1,

    [JsonStringEnumMemberName(nameof(Action))]
    Action = 2,

    [JsonStringEnumMemberName(nameof(Adventure))]
    Adventure = 3,

    [JsonStringEnumMemberName(nameof(Anime))]
    Anime = 4,

    [JsonStringEnumMemberName(nameof(Comedy))]
    Comedy = 5,

    [JsonStringEnumMemberName(nameof(Crime))]
    Crime = 6,

    [JsonStringEnumMemberName(nameof(Documentary))]
    Documentary = 7,

    [JsonStringEnumMemberName(nameof(Drama))]
    Drama = 8,

    [JsonStringEnumMemberName(nameof(Family))]
    Family = 9,

    [JsonStringEnumMemberName(nameof(Fantasy))]
    Fantasy = 10,

    [JsonStringEnumMemberName(nameof(History))]
    History = 11,

    [JsonStringEnumMemberName(nameof(Horror))]
    Horror = 12,

    [JsonStringEnumMemberName(nameof(Music))]
    Music = 13,

    [JsonStringEnumMemberName(nameof(Mystery))]
    Mystery = 14,

    [JsonStringEnumMemberName(nameof(News))]
    News = 15,

    [JsonStringEnumMemberName(nameof(Reality))]
    Reality = 16,

    [JsonStringEnumMemberName(nameof(Romance))]
    Romance = 17,

    [JsonStringEnumMemberName(nameof(ScienceFiction))]
    ScienceFiction = 18,

    [JsonStringEnumMemberName(nameof(Sport))]
    Sport = 19,

    [JsonStringEnumMemberName(nameof(Thriller))]
    Thriller = 20,

    [JsonStringEnumMemberName(nameof(War))]
    War = 21,

    [JsonStringEnumMemberName(nameof(Western))]
    Western = 22,

    [JsonStringEnumMemberName(nameof(Adult))]
    Adult = 23,

    [JsonStringEnumMemberName(nameof(Children))]
    Children = 24,

    [JsonStringEnumMemberName(nameof(Educational))]
    Educational = 25,

    [JsonStringEnumMemberName(nameof(Entertainment))]
    Entertainment = 26,

    [JsonStringEnumMemberName(nameof(GameShow))]
    GameShow = 27,

    [JsonStringEnumMemberName(nameof(TalkShow))]
    TalkShow = 28,

    [JsonStringEnumMemberName(nameof(TVMovie))]
    TVMovie = 29,

    [JsonStringEnumMemberName(nameof(SpecialInterest))]
    SpecialInterest = 30,

    [JsonStringEnumMemberName(nameof(Foreign))]
    Foreign = 31,
}
