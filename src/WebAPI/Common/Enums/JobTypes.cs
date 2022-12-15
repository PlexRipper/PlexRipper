using System.Runtime.Serialization;
using BackgroundServices.InspectPlexServer;

namespace PlexRipper.WebAPI.Common;

public enum JobTypes
{
    // NOTE: Make sure the indexes are correct, 1,2,3,4,5 etc and that there is no skip in between
    // Otherwise the Typescript DTO translator in the front-end starts messing up
    [EnumMember(Value = nameof(InspectPlexServerByPlexAccountIdJob))]
    InspectPlexServerByPlexAccountIdJob = 0,

    [EnumMember(Value = nameof(InspectPlexServerJob))]
    InspectPlexServerJob = 1,
}