namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ViewMode
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [JsonStringEnumMemberName(nameof(Poster))]
    Poster = 0,

    [JsonStringEnumMemberName(nameof(Table))]
    Table = 1,
}
