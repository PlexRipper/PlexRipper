using BackgroundServices.Contracts;
using PlexRipper.WebAPI.Common;

namespace PlexRipper.WebAPI.SignalR.Common;

public class JobStatusUpdateDTO
{
    public string Id { get; set; }

    public string JobName { get; set; }

    public string JobGroup { get; set; }

    public JobTypes JobType { get; set; }

    public TimeSpan JobRuntime { get; set; }

    public DateTime JobStartTime { get; set; }

    public JobStatus Status { get; set; }

    public string PrimaryKey { get; set; }

    public int PrimaryKeyValue { get; set; }
}