using System.Runtime.Serialization;

namespace BackgroundServices.Contracts;

public enum JobStatus
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = nameof(Started))]
    Started = 0,

    [EnumMember(Value = nameof(Running))]
    Running = 1,

    [EnumMember(Value = nameof(Completed))]
    Completed = 2,
}