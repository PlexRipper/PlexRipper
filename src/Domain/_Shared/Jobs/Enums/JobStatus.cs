namespace Reaparr.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobStatus
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc. and that there is no skip in between
    // Otherwise the TypeScript DTO translator in the front-end starts messing up
    [JsonStringEnumMemberName(nameof(Started))]
    Started = 0,

    [JsonStringEnumMemberName(nameof(Completed))]
    Completed = 1,  
    
    [JsonStringEnumMemberName(nameof(Cancelled))]
    Cancelled = 2,   
    
    [JsonStringEnumMemberName(nameof(Failed))]
    Failed = 3,
}
