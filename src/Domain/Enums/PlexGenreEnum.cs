using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlexGenreType
{
    [EnumMember(Value = "Action")]
    Action = 89,

    [EnumMember(Value = "Adventure")]
    Adventure = 17,

    [EnumMember(Value = "Animation")]
    Animation = 18,

    [EnumMember(Value = "Biography")]
    Biography = 4025,

    [EnumMember(Value = "Children")]
    Children = 58220,

    [EnumMember(Value = "Comedy")]
    Comedy = 19,

    [EnumMember(Value = "Crime")]
    Crime = 1057,

    [EnumMember(Value = "Documentary")]
    Documentary = 2268,

    [EnumMember(Value = "Drama")]
    Drama = 91,

    [EnumMember(Value = "Family")]
    Family = 1708,

    [EnumMember(Value = "Fantasy")]
    Fantasy = 92,

    [EnumMember(Value = "History")]
    History = 2568,

    [EnumMember(Value = "Horror")]
    Horror = 551,

    [EnumMember(Value = "Indie")]
    Indie = 16912,

    [EnumMember(Value = "Music")]
    Music = 5114,

    [EnumMember(Value = "Musical")]
    Musical = 2651,

    [EnumMember(Value = "Mystery")]
    Mystery = 552,

    [EnumMember(Value = "Romance")]
    Romance = 2313,

    [EnumMember(Value = "Science Fiction")]
    ScienceFiction = 12286,

    [EnumMember(Value = "Short")]
    Short = 104396,

    [EnumMember(Value = "Sport")]
    Sport = 7484,

    [EnumMember(Value = "Suspense")]
    Suspense = 738,

    [EnumMember(Value = "Thriller")]
    Thriller = 739,

    [EnumMember(Value = "TV Movie")]
    TVMovie = 4599,

    [EnumMember(Value = "War")]
    War = 3773,

    [EnumMember(Value = "Western")]
    Western = 780,
}
